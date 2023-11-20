using Bogus;
using Domain.Entities.Barbers.Service;

namespace DomainTest.Entities.Barbers.Services;

internal class ServiceBuilder
{
    public static Service Build()
    {
        var faker = new Faker();

        var category = new Category(faker.Commerce.Categories(10).ToString() ?? "Any");

        var price = faker.Random.Decimal(0, 500);

        var service = new Service(category, faker.Commerce.ProductName(), faker.Lorem.Text(), price, price * (1 - faker.Random.Decimal((decimal)0.1, (decimal)0.99)), faker.Random.Bool(), faker.Random.Bool(), TimeSpan.FromMinutes(faker.Random.Int(1, 100)));

        return service;
    }

    public static Service Build(TimeSpan duration)
    {
        var faker = new Faker();

        var category = new Category(faker.Commerce.Categories(10).ToString() ?? "Any");

        var price = faker.Random.Decimal(0, 500);

        var service = new Service(category, faker.Commerce.ProductName(), faker.Lorem.Text(), price, price * (1 - faker.Random.Decimal((decimal)0.1, (decimal)0.99)), faker.Random.Bool(), faker.Random.Bool(), duration);

        return service;
    }
}
