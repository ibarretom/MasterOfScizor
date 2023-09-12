using Domain.Entities.Barbers.Service;
using Domain.ValueObjects.Enums;

namespace Domain.Entities.Orders;

internal class Order
{
    public Guid Id { get; }
    public Guid BranchId { get; }
    public Guid WorkerId { get; }
    public List<Service> Services { get; }
    public Guid UserId { get; }
    public OrderStatus Status { get; }
    public DateTime ScheduleTime { get; }
    public DateTime RelocatedSchedule { get; set; }

    public Order(Guid branchId, Guid workerId, List<Service> services, Guid userId, OrderStatus orderStatus, DateTime scheduleTime)
    {
        Id = Guid.NewGuid();
        BranchId = branchId;
        WorkerId = workerId;
        Services = services;
        UserId = userId;
        Status = orderStatus;
        ScheduleTime = scheduleTime;
    }
}
