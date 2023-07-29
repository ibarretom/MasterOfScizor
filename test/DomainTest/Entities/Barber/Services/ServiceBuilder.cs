using Bogus;
using Domain.Entities.Barbers.Service;

namespace DomainTest.Entities.Barber.Services;

internal class ServiceBuilder
{
    public static Service Build()
    {
        var faker = new Faker();

        var category = new Category(faker.Commerce.Categories(10).ToString() ?? "Any", faker.Random.Guid());

        var service = new Service(faker.Random.Guid(), category, faker.Commerce.ProductName(), faker.Lorem.Text(), faker.Random.Bool(), faker.Random.Bool(), TimeSpan.FromMinutes(faker.Random.Int(1, 100)));

        var price = faker.Random.Decimal(0, 500);
        service.SetPrices(price, price * (1 - faker.Random.Decimal((decimal)0.1, (decimal) 0.99)));

        return service;
    }
}
