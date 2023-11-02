using Domain.Entities.Barbers;
using Domain.Entities.Orders;

namespace Application.Services.Orders;

internal interface IOrderPolicy
{
    bool IsAllowed(Order order, List<Order> allOrders, Branch branch, out string reason);
}
