using Bogus;
using Domain.ValueObjects.Addresses;

namespace DomainTest.ValueObjects.Addresses;

internal class AddressLocalizationBuilder
{
    public static AddressLocalization Build()
    {
        var faker = new Faker();

        return new AddressLocalization(faker.Address.Country(), faker.Address.State(), faker.Address.City(), 
                                       faker.Address.County(), faker.Address.StreetName(), faker.Address.ZipCode());
    }
}
