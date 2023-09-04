using Bogus;
using Bogus.Extensions.Brazil;
using Domain.Entities.Barbers;
using Domain.ValueObjects.Addresses;
using Domain.ValueObjects.Enums;

namespace DomainTest.Entities.Barbers;

internal class BarberBuilder
{
    public static Barber Build()
    {
        var faker = new Faker();
        
        var cnpj = faker.Company.Cnpj();
        var configuration = new Configuration(faker.Random.Enum<OrderQueueType>(), faker.Random.Bool(), faker.Random.Int(0, 1000), new Schedule(DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Monday), new TimeSpan(faker.Random.Int(0, 60)), faker.Random.Bool());
        var branch = new Branch(Guid.NewGuid(), cnpj, new Address(Guid.NewGuid(), new AddressIdentifier(faker.Random.Number(1, 300).ToString(), faker.Random.Number(0).ToString())),
            faker.Phone.PhoneNumber(), faker.Internet.Email(), false, configuration);

        var barber = new Barber(Guid.NewGuid(), faker.Company.CompanyName(), cnpj, string.Empty);
        barber.AddBranch(branch);

        return barber;
    }
}
