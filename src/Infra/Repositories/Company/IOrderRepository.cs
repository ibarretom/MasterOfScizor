using Domain.Entities.Orders;

namespace Infra.Repositories.Company;

internal interface IOrderRepository
{
    public Task Create(Order order);
}
