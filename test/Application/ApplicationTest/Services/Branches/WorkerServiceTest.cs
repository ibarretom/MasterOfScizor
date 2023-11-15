using Application.Services.Branches;
using Domain.Entities;
using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Authentication;
using Domain.Services.Barbers;
using Domain.ValueObjects.Barbers;
using Domain.ValueObjects.Enums;
using DomainTest.Entities;
using DomainTest.Entities.Barbers;
using DomainTest.Entities.Barbers.Services;
using DomainTest.ValueObjects.DTO;
using Infra.Repositories.Company;
using Moq;

namespace ApplicationTest.Services.Branches;

public class WorkerServiceTest
{
    [Fact]
    public async void ShouldBeAbleToAddAWorkerToABranch()
    {
        var worker = EmployeeCreateRequestDTOBuilder.Build();
        var createdWorker = EmployeeBuilder.Build();

        var workerRepository = new Mock<IWorkerRepository>();
        workerRepository.Setup(repository => repository.Exists(worker.BranchId, worker.Document, worker.Phone, worker.Email).Result).Returns(false);
        workerRepository.Setup(repository => repository.Add(It.IsAny<Employee>()))
            .Callback<Employee>(employee =>
            {
                createdWorker = employee;
            }).Returns(Task.CompletedTask);

        var encriptService = new Mock<IEncryptService>();
        encriptService.Setup(service => service.Hash(worker.Email + worker.Password)).Returns("hash");

        var scheduler = new Mock<IScheduler>();
        var orderRepository = new Mock<IOrderRepository>();

        var workerService = new WorkerService(workerRepository.Object, encriptService.Object, orderRepository.Object, scheduler.Object);
        await workerService.Add(worker);

        Assert.True(!createdWorker.Id.Equals(Guid.Empty));
        Assert.True(createdWorker.Name.Equals(worker.Name));
        Assert.True(createdWorker.Email.Equals(worker.Email));
        Assert.True(createdWorker.Phone.Equals(worker.Phone));
        Assert.True(createdWorker.Document.Equals(worker.Document));
        Assert.True(createdWorker.Avatar.Equals(worker.Avatar));
        Assert.True(createdWorker.Active.Equals(worker.Active));
        Assert.True(createdWorker.Password.Equals("hash"));
        Assert.Contains(UserRole.Customer, createdWorker.Roles);
        Assert.True(createdWorker.Roles.Count.Equals(worker.UserRoles.Count + 1));
    }

    [Fact]
    public async void ShouldNotBeAbleToAddAWorkerToABranchWhenWorkerAlreadyExists()
    {
        var worker = EmployeeCreateRequestDTOBuilder.Build();
        var createdWorker = EmployeeBuilder.Build();

        var workerRepository = new Mock<IWorkerRepository>();
        workerRepository.Setup(repository => repository.Exists(worker.BranchId, worker.Document, worker.Phone, worker.Email).Result).Returns(true);

        var encriptService = new Mock<IEncryptService>();

        var scheduler = new Mock<IScheduler>();
        var orderRepository = new Mock<IOrderRepository>();

        var workerService = new WorkerService(workerRepository.Object, encriptService.Object, orderRepository.Object, scheduler.Object);

        var exception = await Assert.ThrowsAsync<CompanyException>(() => workerService.Add(worker));
        Assert.Equal(CompanyExceptionMessagesResource.WORKER_ALREADY_EXISTS, exception.Message);

    }

    [Fact]
    public async void ShouldBeAbleToAddServicesToAWorker()
    {
        var services = new List<Service> { ServiceBuilder.Build(), ServiceBuilder.Build() };
        var employee = EmployeeBuilder.Build();

        var workerRepository = new Mock<IWorkerRepository>();
        workerRepository.Setup(repository => repository.Update(It.IsAny<Employee>()))
            .Callback<Employee>(worker =>
            {
                employee = worker;
            }).Returns(Task.CompletedTask);

        var encryptService = new Mock<IEncryptService>();

        var scheduler = new Mock<IScheduler>();
        var orderRepository = new Mock<IOrderRepository>();

        var workerService = new WorkerService(workerRepository.Object, encryptService.Object, orderRepository.Object, scheduler.Object);

        await workerService.Add(services, employee);

        Assert.True(employee.Services.Count.Equals(services.Count));
    }

    [Fact]
    public async Task ShouldIgnoreDuplicateServices()
    {
        var services = new List<Service> { ServiceBuilder.Build(), ServiceBuilder.Build() };
        var employee = EmployeeBuilder.Build();
        employee.AddServices(services);

        var workerRepository = new Mock<IWorkerRepository>();
        workerRepository.Setup(repository => repository.Update(It.IsAny<Employee>()))
            .Callback<Employee>(worker =>
            {
                employee = worker;
            }).Returns(Task.CompletedTask);

        var encryptService = new Mock<IEncryptService>();
        var scheduler = new Mock<IScheduler>();
        var orderRepository = new Mock<IOrderRepository>();

        var workerService = new WorkerService(workerRepository.Object, encryptService.Object, orderRepository.Object, scheduler.Object);

        var servicesToAdd = services.Concat(new List<Service> { ServiceBuilder.Build() }).ToList();
        await workerService.Add(servicesToAdd, employee);

        Assert.True(employee.Services.Count.Equals(services.Count + 1));

        foreach (var service in services)
        { 
            Assert.Contains(service, employee.Services);
        }
    }

    [Fact]
    public async Task ShouldBeAbleToRetrieveAvailableTimesForAServiceRequest()
    {
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay());

        var now = DateTimeFactory.BuildWithoutSeconds(DateTime.UtcNow);
        var nowPlusSomeHour = now.AddHours(1);

        var schedule = new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(nowPlusSomeHour.Hour, nowPlusSomeHour.Minute), now.DayOfWeek);
        branch.AddSchedule(schedule);

        var scheduleTime = new ScheduleTimes(now, nowPlusSomeHour);

        var worker = EmployeeBuilder.Build();
        branch.AddEmployee(worker);

        var workerRepository = new Mock<IWorkerRepository>();
        
        var encryptService = new Mock<IEncryptService>();
        
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(repository => repository.GetBy(branch.Id, worker.Id, scheduleTime)).ReturnsAsync(new List<Order>());

        var scheduler = new Mock<IScheduler>();
        scheduler.Setup(schedule => schedule.GetAvailable(now, It.IsAny<OrderBase>(), It.IsAny<List<Order>>())).Returns(new HashSet<DateTime>() { now });

        var workerService = new WorkerService(workerRepository.Object, encryptService.Object, orderRepository.Object, scheduler.Object);

        var service = ServiceBuilder.Build();
        branch.AddService(service);
        branch.AddService(service.Id, worker.Id);

        var orderBase = new OrderBase(branch, worker, new List<Service>() { service });

        var availableTimes = await workerService.GetAvailableTimes(now, orderBase);

        orderRepository.Verify(repository => repository.GetBy(branch.Id, worker.Id, scheduleTime), Times.Once);

        Assert.Single(availableTimes);
        Assert.Contains(now, availableTimes);
    }

    [Fact]
    public async Task ShouldPassCorrectlyTheScheduleTimeToTheOrderRepositoryWhenHasACrossingSchedule()
    {
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay());

        var now = DateTime.UtcNow;
        var timeForReference = new DateTime(now.Year, now.Month, now.Minute, 23, 0, 0, now.Kind);

        var TimeReferencePlusSomeHour = timeForReference.AddHours(2);

        var schedule = new Schedule(new TimeOnly(timeForReference.Hour, timeForReference.Minute), 
                                    new TimeOnly(TimeReferencePlusSomeHour.Hour, TimeReferencePlusSomeHour.Minute), timeForReference.DayOfWeek);
        branch.AddSchedule(schedule);

        var nextDaySchedule = new Schedule(new TimeOnly(timeForReference.AddDays(1).Hour, timeForReference.AddDays(1).Minute),
                                           new TimeOnly(TimeReferencePlusSomeHour.AddDays(1).Hour, TimeReferencePlusSomeHour.AddDays(1).Minute), 
                                           timeForReference.AddDays(1).DayOfWeek);
        branch.AddSchedule(nextDaySchedule);

        var scheduleTime = new ScheduleTimes(timeForReference, TimeReferencePlusSomeHour.AddDays(1));

        var worker = EmployeeBuilder.Build();
        branch.AddEmployee(worker);

        var workerRepository = new Mock<IWorkerRepository>();

        var encryptService = new Mock<IEncryptService>();

        var orderRepository = new Mock<IOrderRepository>();

        var scheduler = new Mock<IScheduler>();

        var workerService = new WorkerService(workerRepository.Object, encryptService.Object, orderRepository.Object, scheduler.Object);

        var service = ServiceBuilder.Build();
        branch.AddService(service);
        branch.AddService(service.Id, worker.Id);

        var orderBase = new OrderBase(branch, worker, new List<Service>() { service });

        await workerService.GetAvailableTimes(timeForReference.AddHours(4), orderBase);

        orderRepository.Verify(repository => repository.GetBy(branch.Id, worker.Id, scheduleTime), Times.Once);
    }

    [Fact]
    public async Task ShouldThrowsWhenThereIsNoScheduleForTheDay()
    {
        var branch = BranchBuilder.Build(ConfigurationBuilder.BuildWithScheduleWithNoDelay());

        var worker = EmployeeBuilder.Build();
        branch.AddEmployee(worker);

        var workerRepository = new Mock<IWorkerRepository>();

        var encryptService = new Mock<IEncryptService>();

        var orderRepository = new Mock<IOrderRepository>();

        var scheduler = new Mock<IScheduler>();

        var workerService = new WorkerService(workerRepository.Object, encryptService.Object, orderRepository.Object, scheduler.Object);

        var service = ServiceBuilder.Build();
        branch.AddService(service);
        branch.AddService(service.Id, worker.Id);

        var orderBase = new OrderBase(branch, worker, new List<Service>() { service });

        var exception = Assert.ThrowsAsync<CompanyException>(async () => await workerService.GetAvailableTimes(DateTime.UtcNow, orderBase));

        Assert.Equal(CompanyExceptionMessagesResource.BRANCH_IS_NOT_OPENNED_THIS_DAY, exception.Result.Message);
    }
}
