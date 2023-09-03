using Domain.Entities.Orders;

namespace Infra.Repositories.Orders;

internal interface IOrderRepository
{
    public void Create(Order order);
}
