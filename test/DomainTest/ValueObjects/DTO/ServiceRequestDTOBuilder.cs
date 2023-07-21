using Bogus;
using Domain.Entities.Barbers.Service;
using Domain.ValueObjects.DTO.Barber;

namespace DomainTest.ValueObjects.DTO;

internal class ServiceRequestDTOBuilder
{
    public static ServiceRequestDTO Build()
    {
        var faker = new Faker();

        return new ServiceRequestDTO(faker.Random.Guid(), new Category(faker.Commerce.Categories(10).ToString() ?? "Any", faker.Random.Guid()),
                faker.Commerce.ProductName(), faker.Commerce.ProductDescription(), faker.Random.Decimal(1, 100), faker.Random.Decimal(1, 100),
                faker.Random.Bool(), faker.Random.Bool(), TimeSpan.FromMinutes(faker.Random.Int(1, 100)));
    }
}
