using Domain.Entities.Barbers;
using Domain.Entities.Orders;

namespace Application.Services.Orders;

internal interface IOrderTool
{
    DateTime RelocateTime(Order order, List<Order> allOrders, Branch branch);
}
