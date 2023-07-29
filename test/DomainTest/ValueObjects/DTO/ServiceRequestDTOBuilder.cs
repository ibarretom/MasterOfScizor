using Bogus;
using Domain.Entities.Barbers.Service;
using Domain.ValueObjects.DTO.Barber;

namespace DomainTest.ValueObjects.DTO;

internal class ServiceRequestDTOBuilder
{
    public static ServiceRequestDTO Build()
    {
        var faker = new Faker();

        var price = faker.Random.Decimal(1, 100);
        var promotionalPrice = price * (1 - faker.Random.Decimal((decimal)0.1, (decimal)0.99));
        
        return new ServiceRequestDTO(faker.Random.Guid(), new Category(faker.Commerce.Categories(10).ToString() ?? "Any", faker.Random.Guid()),
                faker.Commerce.ProductName(), faker.Commerce.ProductDescription(), price, promotionalPrice,
                faker.Random.Bool(), faker.Random.Bool(), TimeSpan.FromMinutes(faker.Random.Int(1, 100)));
    }
}
