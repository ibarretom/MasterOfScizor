using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.ValueObjects.Enums;

namespace DomainTest.Entities.Orders;

internal class OrderBuilder
{
    public static Order Build()
    {
        return new Order(Guid.NewGuid(), Guid.NewGuid(), new List<Service>(), Guid.NewGuid(), OrderStatus.Pending, DateTime.Now);
    }

    public static Order Build(DateTime orderTime)
    {
        return new Order(Guid.NewGuid(), Guid.NewGuid(), new List<Service>(), Guid.NewGuid(), OrderStatus.Pending, orderTime);
    }

    public static Order Build(DateTime orderTime, Guid workerId)
    {
        return new Order(Guid.NewGuid(), workerId, new List<Service>(), Guid.NewGuid(), OrderStatus.Pending, orderTime);
    }

    public static Order Build(DateTime orderTime, Guid workerId, List<Service> services)
    {
        var order = new Order(Guid.NewGuid(), workerId, services, Guid.NewGuid(), OrderStatus.Pending, orderTime)
        {
            RelocatedSchedule = orderTime
        };

        return order;
    }
}
