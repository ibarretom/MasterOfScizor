using Domain.Entities;
using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Authentication;
using Domain.ValueObjects.DTO;
using Domain.ValueObjects.Enums;
using Infra.Repositories.Company;

namespace Application.Services.Branches;

internal class WorkerService
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IEncriptService _encryptService;

    public WorkerService(IWorkerRepository workerRepository, IEncriptService encryptService)
    {
        _workerRepository = workerRepository;
        _encryptService = encryptService;
    }

    public async Task Add(EmployeeCreateRequestDTO worker)
    {
        if (await _workerRepository.Exists(worker.BranchId, worker.Document, worker.Phone, worker.Email))
            throw new CompanyException(CompanyExceptionMessagesResource.WORKER_ALREADY_EXISTS);

        var workerCreated = new Employee(worker.BranchId, worker.Active, worker.Avatar, worker.Document, worker.Name, worker.Email, worker.Phone);
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
}
