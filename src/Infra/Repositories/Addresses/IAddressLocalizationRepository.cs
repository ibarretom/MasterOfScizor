using Domain.ValueObjects.Addresses;

namespace Infra.Repositories.Addresses;

internal interface IAddressLocalizationRepository
{
    Task<AddressLocalization> GetById(Guid addressId);
}
