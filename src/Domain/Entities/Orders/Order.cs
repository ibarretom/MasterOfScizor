using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.ValueObjects.Enums;

namespace Domain.Entities.Orders;

internal class Order : OrderBase
{
    public User User { get; }
    public OrderStatus Status { get; private set; }
    public DateTime ScheduleTime { get; }
    public DateTime RelocatedSchedule { get; set; }
    public decimal Total { get; private set; }

    public Order(Branch branch, Employee worker, List<Service> services, User user, OrderStatus orderStatus, DateTime scheduleTime)
          : base(branch, worker, services)
    {
        User = user;
        Status = orderStatus;
        ScheduleTime = scheduleTime;
        RelocatedSchedule = scheduleTime;
        Total = CalculateTotal();
    }

    public decimal CalculateTotal()
    {
        return Services.Sum(service => service.Price);
    }

    public void UpdateStatus(OrderStatus orderStatus)
    {
        Status = orderStatus;
    }
}
