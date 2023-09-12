using Domain.Entities.Barbers;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.DTO.Barber;
using Domain.ValueObjects.Enums;
using Infra.Repositories.Addresses;
using Infra.Repositories.CompanyRepository;

namespace Application.Services.Company;

internal class CompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAddressLocalizationRepository _addressLocalizationRepository;

    public CompanyService(ICompanyRepository companyRepository, IAddressLocalizationRepository addressLocalizationRepository)
    {
        _companyRepository = companyRepository;
        _addressLocalizationRepository = addressLocalizationRepository;
    }

    public async Task Create(CreateCompanyRequestDTO company)
    {
        if (await _addressLocalizationRepository.GetById(company.Branch.Address.AddressId) is null)
            throw new AddressException(AddressExceptionMessagesResource.ADDRESS_NOT_FOUND);

        if(await _companyRepository.GetByCompanyIdentifier(company.Identifier) is not null)
            throw new CompanyException(CompanyExceptionMessagesResource.COMPANY_ALREADY_EXISTS);

        var companyCreated = new Barber(company.OwnerId, company.Name, company.Identifier, string.Empty);

        var branchConfig = new Configuration(OrderQueueType.Schedule, true, null, new TimeSpan(20), true);

        companyCreated.AddBranch(new Branch(companyCreated.Id, company.Branch.Identifier, company.Branch.Address, company.Branch.Phone, company.Branch.Email, false, branchConfig));
        
        await _companyRepository.Create(companyCreated);
    }

    public async Task SetAvatar(Guid companyId, string avatar)
    {
        var company = await _companyRepository.GetById(companyId) ?? throw new CompanyException(CompanyExceptionMessagesResource.COMPANY_NOT_FOUND);

        company.Avatar = avatar;
        
        await _companyRepository.Update(company);
    }
}
