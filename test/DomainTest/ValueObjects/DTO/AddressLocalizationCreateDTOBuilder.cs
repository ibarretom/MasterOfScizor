using Bogus;
using Domain.ValueObjects.DTO;

namespace DomainTest.ValueObjects.DTO;

internal class AddressLocalizationCreateDTOBuilder
{
    public static AddressLocalizationCreateDTO Build()
    {
        var faker = new Faker();

        return new AddressLocalizationCreateDTO(faker.Address.Country(), faker.Address.State(), faker.Address.City(), faker.Address.City(), faker.Address.StreetName(), faker.Address.ZipCode());
    }
}
