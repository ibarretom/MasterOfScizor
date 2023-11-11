using Domain.Entities;
using Domain.Entities.Barbers;
using Domain.Entities.Orders;

namespace Application.Services.Orders;

internal interface IOrderTime
{
    public HashSet<DateTime> GetAvailable(DateTime day, Employee employee, List<Order> orders, Branch branch); 
}
