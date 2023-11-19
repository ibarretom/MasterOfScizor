using Domain.Entities;
using Domain.Entities.Barbers;
using Domain.Entities.Orders;

namespace Domain.Services.Barbers;

internal interface IScheduler
{
    public HashSet<DateTime> GetAvailable(DateTime day, OrderBase orderBase, List<Order> orders);
    public HashSet<TimeOnly> GetAllPossible(Schedule schedule, Configuration configuration);
}
