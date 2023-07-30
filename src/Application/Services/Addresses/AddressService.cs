using Domain.ValueObjects.Addresses;
using Domain.ValueObjects.DTO;
using Infra.Repositories.Addresses;

namespace Application.Services.Addresses;

internal class AddressService
{
    private readonly IAddressLocalizationRepository _addressLocalizationRepository;

    public AddressService(IAddressLocalizationRepository addressLocalizationRepository)
    {
        _addressLocalizationRepository = addressLocalizationRepository;
    }

    public async Task<AddressLocalization> AddOrRetrieve(AddressLocalizationCreateDTO address) 
    {
        var registredAddress = await _addressLocalizationRepository.GetByZipCode(address.ZipCode);

        if (registredAddress is not null)
            return registredAddress;

        registredAddress = new AddressLocalization(address.Country, address.State, address.City, address.Neighborhood, address.Street, address.ZipCode);

        registredAddress = await _addressLocalizationRepository.Add(registredAddress);

        return registredAddress;
    }
}
