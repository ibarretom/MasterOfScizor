using Bogus;
using Domain.ValueObjects.DTO.Orders;
using DomainTest.Entities.Barbers.Services;

namespace DomainTest.ValueObjects.DTO;

internal class OrderRequestDTOBuilder
{
    public static OrderRequestDTO Build(int? servicesAmount = null)
    {
        var faker = new Faker();

        return new OrderRequestDTO(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Enumerable.Range(0, servicesAmount ?? faker.Random.Int(0, 5)).Select(num => ServiceBuilder.Build()).ToList(),
            Guid.NewGuid(),
            DateTime.Now
        );
    }
}
