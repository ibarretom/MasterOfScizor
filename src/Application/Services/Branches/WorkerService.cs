using Domain.Entities;
using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Authentication;
using Domain.Services.Barbers;
using Domain.ValueObjects.Barbers;
using Domain.ValueObjects.DTO;
using Domain.ValueObjects.Enums;
using Infra.Repositories.Company;

namespace Application.Services.Branches;

internal class WorkerService
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IEncryptService _encryptService;
    private readonly IOrderRepository _orderRepository;
    private readonly IScheduler _scheduler;

    public WorkerService(IWorkerRepository workerRepository, IEncryptService encryptService, IOrderRepository orderRepository, IScheduler scheduler)
    {
        _workerRepository = workerRepository;
        _encryptService = encryptService;
        _scheduler = scheduler;
        _orderRepository = orderRepository;
    }

    public async Task Add(EmployeeCreateRequestDTO worker)
    {
        if (await _workerRepository.Exists(worker.Branch.Id, worker.Document, worker.Phone, worker.Email))
            throw new CompanyException(CompanyExceptionMessagesResource.WORKER_ALREADY_EXISTS);

        var workerCreated = new Employee(worker.Branch, worker.Active, worker.Avatar, worker.Document, worker.Name, worker.Email, worker.Phone);
        workerCreated.SetPassword(worker.Password, _encryptService);

        workerCreated.AddRoles(UserRole.Customer);
        workerCreated.AddRoles(worker.UserRoles);

        await _workerRepository.Add(workerCreated);
    }

    public async Task Add(List<Service> services, Employee employee)
    {
        employee.AddServices(services);

        await _workerRepository.Update(employee);
    }

    public async Task<HashSet<DateTime>> GetAvailableTimes(DateTime day, OrderBase order)
    {
        var scheduleForThisDay = order.Branch.GetScheduleFor(day.DayOfWeek);

        if (!scheduleForThisDay.Any())
            throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY);

        var schedulesTimes = scheduleForThisDay.Select(schedule => Schedule.GetScheduleDateTime(schedule, schedule.GetDateToBeReference(day))).ToList();

        var (StartTime, _) = schedulesTimes
                        .MinBy(schedule => schedule.StartTime);
        var (_, EndTime) = schedulesTimes
                        .MaxBy(schedule => schedule.EndTime);

        var orders = await _orderRepository.GetBy(order.Branch.Id, order.Worker.Id, new ScheduleTimes(StartTime, EndTime));

        return _scheduler.GetAvailable(day, order, orders);
    }
}
