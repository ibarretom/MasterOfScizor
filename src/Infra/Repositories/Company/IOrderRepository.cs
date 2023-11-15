using Domain.Entities.Orders;
using Domain.ValueObjects.Barbers;

namespace Infra.Repositories.Company;

internal interface IOrderRepository
{
    public Task Create(Order order);
    public Task<List<Order>> GetBy(Guid branchId, Guid workerId);
    public Task<List<Order>> GetBy(Guid branchId, Guid workerId, ScheduleTimes date);
    public Task Update(Order order);
}
