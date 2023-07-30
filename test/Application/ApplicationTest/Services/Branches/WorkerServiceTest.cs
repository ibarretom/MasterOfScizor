using Application.Services.Branches;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Authentication;
using Domain.ValueObjects;
using DomainTest.Entities;
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

        var encriptService = new Mock<IEncriptService>();
        encriptService.Setup(service => service.Hash(worker.Email + worker.Password)).Returns("hash");


        var workerService = new WorkerService(workerRepository.Object, encriptService.Object);
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

        var encriptService = new Mock<IEncriptService>();

        var workerService = new WorkerService(workerRepository.Object, encriptService.Object);

        var exception = await Assert.ThrowsAsync<CompanyException>(() => workerService.Add(worker));
        Assert.Equal(CompanyExceptionMessagesResource.WORKER_ALREADY_EXISTS, exception.Message);

    }
}
