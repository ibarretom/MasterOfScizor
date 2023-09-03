using Domain.Entities.Barbers.Service;
using Domain.ValueObjects.Enums;

namespace Domain.Entities.Orders;

internal class Order
{
    public Guid Id { get; }
    public Guid BranchId { get; }
    public Guid WorkerId { get; }
    public List<Guid> ServiceId { get; }
    public Guid UserId { get; }
    public OrderStatus Status { get; }
    public DateTime ScheduleTime { get; }

    public Order(Guid branchId, Guid workerId, List<Guid> serviceId, Guid userId, OrderStatus orderStatus, DateTime scheduleTime)
    {
        Id = Guid.NewGuid();
        BranchId = branchId;
        WorkerId = workerId;
        ServiceId = serviceId;
        UserId = userId;
        Status = orderStatus;
        ScheduleTime = scheduleTime;
    }
}
