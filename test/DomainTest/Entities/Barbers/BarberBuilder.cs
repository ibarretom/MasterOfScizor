using Bogus;
using Bogus.Extensions.Brazil;
using Domain.Entities.Barbers;
using Domain.ValueObjects.Addresses;
using DomainTest.ValueObjects.Addresses;

namespace DomainTest.Entities.Barbers;

internal class BarberBuilder
{
    public static Barber Build()
    {
        var faker = new Faker();
        
        var cnpj = faker.Company.Cnpj();
        var configuration = ConfigurationBuilder.BuildRandom();
        var address = AddressLocalizationBuilder.Build();
        var branch = new Branch(cnpj, new Address(address, new AddressIdentifier(faker.Random.Number(1, 300).ToString(), faker.Random.Number(0).ToString())),
            faker.Phone.PhoneNumber(), faker.Internet.Email(), false, configuration);

        var user = UserBuilder.Build();
        var barber = new Barber(user, faker.Company.CompanyName(), cnpj, string.Empty);
        barber.AddBranch(branch);

        return barber;
    }
}
