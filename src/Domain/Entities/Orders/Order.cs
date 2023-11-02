using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.ValueObjects.Enums;
using System.Data;

namespace Domain.Entities.Orders;

internal class Order
{
    public Guid Id { get; }
    public Branch Branch { get; }
    public Employee Worker { get; }
    public List<Service> Services { get; }
    public User User { get; }
    public OrderStatus Status { get; private set; }
    public DateTime ScheduleTime { get; }
    public DateTime RelocatedSchedule { get; set; }

    public Order(Branch branch, Employee worker, List<Service> services, User user, OrderStatus orderStatus, DateTime scheduleTime)
    {
        Id = Guid.NewGuid();
        Branch = branch;
        Worker = worker;
        Services = services;
        User = user;
        Status = orderStatus;
        ScheduleTime = scheduleTime;
        RelocatedSchedule = scheduleTime;
    }

    public void UpdateStatus(OrderStatus orderStatus)
    {
        Status = orderStatus;
    }
}
