using Domain.Entities.Barbers;
using Domain.ValueObjects.DTO.Barber;
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
            throw new Exception("Address not found");

        if(await _companyRepository.GetByCompanyIdentifier(company.Identifier) is not null)
            throw new Exception("Company already exists");

        var companyCreated = new Barber(company.OwnerId, company.Name, company.Identifier, string.Empty);
        companyCreated.AddBranch(new Branch(companyCreated.Id, company.Branch.Identifier, company.Branch.Address, company.Branch.Phone, company.Branch.Email, false));
        
        await _companyRepository.Create(companyCreated);
    }
}
