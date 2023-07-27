using Domain.Entities.Barbers.Service;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.DTO.Barber;
using Infra.Repositories.Company;
using Infra.Repositories.CompanyRepository;

namespace Application.Services.Branches;

internal class BranchService
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IBranchRepository _branchRepository;

    public BranchService(IServiceRepository serviceRepository, IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task AddService(ServiceRequestDTO service)
    {
        if (!(await _branchRepository.Exists(service.BranchId)))
            throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_NOT_FOUND);

        if (await _serviceRepository.Exists(service.BranchId, service.Name, service.Category?.Id ?? Guid.Empty))
            throw new CompanyException(CompanyExceptionMessagesResource.SERVICE_ALREADY_EXISTS);

        var serviceCreated = new Service(service.BranchId, service.Category is not null ? new Category(service.Category.Name, service.Category.CompanyId) : null, service.Name, service.Description,
                service.Price, service.PromotionalPrice, service.IsPromotionActive, service.Active, service.Duration);

        await _serviceRepository.Add(serviceCreated);
    }
}
