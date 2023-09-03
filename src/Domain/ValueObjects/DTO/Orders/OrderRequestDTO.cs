namespace Domain.ValueObjects.DTO.Orders;

internal class OrderRequestDTO
{
    public Guid BranchId { get; }
    public Guid WorkerId { get;  }
    public List<Guid> ServiceId { get; }
    public Guid UserId { get; }
    public DateTime ScheduleTime { get; }

    public OrderRequestDTO(Guid branchId, Guid workerId, List<Guid> serviceId, Guid userId, DateTime scheduleTime)
    {
        BranchId = branchId;
        WorkerId = workerId;
        ServiceId = serviceId;
        UserId = userId;
        ScheduleTime = scheduleTime;
    }
}
