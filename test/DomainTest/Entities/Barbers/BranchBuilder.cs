using Bogus;
using Bogus.Extensions.Brazil;
using Domain.Entities.Barbers;
using Domain.ValueObjects.Addresses;

namespace DomainTest.Entities.Barbers;

internal class BranchBuilder
{

    public static Branch Build(Configuration configuration, bool isOpened = false)
    {
        var faker = new Faker();

        var addressIdentifier = new AddressIdentifier(faker.Address.BuildingNumber(), faker.Address.BuildingNumber());
        var address = new Address(Guid.NewGuid(), addressIdentifier);
  
        return new Branch(faker.Company.Cnpj(), address, faker.Phone.PhoneNumber(), faker.Internet.Email(), isOpened, configuration);
    }
}
