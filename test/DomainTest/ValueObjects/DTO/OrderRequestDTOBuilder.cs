using Bogus;
using Domain.ValueObjects.DTO.Orders;

namespace DomainTest.ValueObjects.DTO;

internal class OrderRequestDTOBuilder
{
    public static OrderRequestDTO Build()
    {
        var faker = new Faker();

        return new OrderRequestDTO(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Enumerable.Range(0, faker.Random.Int(0, 5)).Select(num => Guid.NewGuid()).ToList(),
            Guid.NewGuid(),
            DateTime.Now
        );
    }

    public static OrderRequestDTO Build(List<Guid> serviceIds)
    {
        return new OrderRequestDTO(Guid.NewGuid(), Guid.NewGuid(), serviceIds, Guid.NewGuid(), DateTime.Now);
    }
}
