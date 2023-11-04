using Domain.Entities;
using Domain.Entities.Barbers;
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
    private readonly ICategoryRepository _categoryRepository;

    public BranchService(IServiceRepository serviceRepository, IBranchRepository branchRepository, ICategoryRepository categoryRepository)
    {
        _branchRepository = branchRepository;
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task Add(ServiceRequestDTO service)
    {
        if (!(await _branchRepository.Exists(service.BranchId)))
            throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_NOT_FOUND);

        if (await _serviceRepository.Exists(service.BranchId, service.Name, service.Category?.Id ?? Guid.Empty))
            throw new CompanyException(CompanyExceptionMessagesResource.SERVICE_ALREADY_EXISTS);

        var serviceCreated = new Service(service.BranchId, service.Category is not null ? new Category(service.Category.Name, service.Category.CompanyId) : null, service.Name, service.Description, service.Price, service.PromotionalPrice, service.IsPromotionActive, service.Active, service.Duration);

        await _serviceRepository.Add(serviceCreated);
    }

    public async Task Update(ServiceUpdateRequestDTO service)
    {
        var existentService = await _serviceRepository.GetById(service.BranchId, service.Id) ?? throw new CompanyException(CompanyExceptionMessagesResource.SERVICE_NOT_FOUND);

        if (service.Category is not null && !(await _categoryRepository.Exists(service.BranchId, service.Category.Id)))
            throw new ServiceException(ServiceExceptionMessagesResource.CATEGORY_NOT_FOUND);

        existentService.Name = service.Name;
        existentService.Description = service.Description;
        existentService.IsPromotionActive = service.IsPromotionActive;
        existentService.Active = service.Active;
        existentService.Duration = service.Duration;
        existentService.Category = service.Category is not null ? service.Category : null;

        existentService.SetPrices(service.Price, service.PromotionalPrice);

        await _serviceRepository.Update(existentService);
    }

    public async Task Disable(Guid branchId, Guid serviceId)
    {
        if (!(await _branchRepository.Exists(branchId)))
            throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_NOT_FOUND);

        var serviceToDisable = await _serviceRepository.GetById(branchId, serviceId) ?? throw new CompanyException(CompanyExceptionMessagesResource.SERVICE_NOT_FOUND);

        serviceToDisable.Active = false;

        await _serviceRepository.Update(serviceToDisable);
    }

    public async Task<ServiceGetResponseDTO> GetSingle(Guid branchId, Guid serviceId)
    {
        if (!(await _branchRepository.Exists(branchId)))
            throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_NOT_FOUND);

        var service = await _serviceRepository.GetById(branchId, serviceId);

        return new ServiceGetResponseDTO(new List<Service>() { service });
    }

    public async Task<ServiceGetResponseDTO> GetAll(Guid branchId)
    {
        if (!(await _branchRepository.Exists(branchId)))
            throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_NOT_FOUND);

        var services = await _serviceRepository.GetAll(branchId);

        return new ServiceGetResponseDTO(services);
    }

    public async Task Add(LunchIntervalDTO lunchIntervalDTO)
    {
        var branch = await _branchRepository.GetBy(lunchIntervalDTO.BranchId) ?? throw new CompanyException(CompanyExceptionMessagesResource.BRANCH_NOT_FOUND);

        branch.AddEmployeeLunchInterval(new Schedule(lunchIntervalDTO.StartTime, lunchIntervalDTO.EndTime, lunchIntervalDTO.WeekDay), lunchIntervalDTO.EmployeeId);

        await _branchRepository.Update(branch);
    }
}
