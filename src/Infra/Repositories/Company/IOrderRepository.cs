using Domain.Entities.Orders;

namespace Infra.Repositories.Company;

internal interface IOrderRepository
{
    public Task Create(Order order);
    public Task<List<Order>> GetBy(Guid branchId, Guid workerId);
    public Task Update(Order order);
}
