using Domain.Entities;
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
    private readonly IEncriptService _encriptService;

    public WorkerService(IWorkerRepository workerRepository, IEncriptService encriptService)
    {
        _workerRepository = workerRepository;
        _encriptService = encriptService;
    }

    public async Task Add(EmployeeCreateRequestDTO worker)
    {
        if (await _workerRepository.Exists(worker.BranchId, worker.Document, worker.Phone, worker.Email))
            throw new CompanyException(CompanyExceptionMessagesResource.WORKER_ALREADY_EXISTS);

        var workerCreated = new Employee(worker.BranchId, worker.Active, worker.Avatar, worker.Document, worker.Name, worker.Email, worker.Phone);
        workerCreated.SetPassword(worker.Password, _encriptService);

        workerCreated.AddRoles(UserRole.Customer);
        workerCreated.AddRoles(worker.UserRoles);

        await _workerRepository.Add(workerCreated);
    }
}
