using Domain.Entities.Barbers.Service;

namespace Domain.ValueObjects.DTO.Orders;

internal class OrderRequestDTO
{
    public Guid BranchId { get; }
    public Guid WorkerId { get;  }
    public List<Service> Services { get; }
    public Guid UserId { get; }
    public DateTime ScheduleTime { get; }

    public OrderRequestDTO(Guid branchId, Guid workerId, List<Service> services, Guid userId, DateTime scheduleTime)
    {
        BranchId = branchId;
        WorkerId = workerId;
        Services = services;
        UserId = userId;
        ScheduleTime = scheduleTime;
    }
}
