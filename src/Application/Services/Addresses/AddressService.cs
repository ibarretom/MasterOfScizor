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
        var registeredAddress = await _addressLocalizationRepository.GetByZipCode(address.ZipCode);

        if (registeredAddress is not null)
            return registeredAddress;

        registeredAddress = new AddressLocalization(address.Country, address.State, address.City, address.Neighborhood, address.Street, address.ZipCode);

        registeredAddress = await _addressLocalizationRepository.Add(registeredAddress);

        return registeredAddress;
    }
}
