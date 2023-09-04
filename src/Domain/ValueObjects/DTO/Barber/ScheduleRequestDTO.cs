namespace Domain.ValueObjects.DTO.Barber;

internal class ScheduleRequestDTO
{
    public Guid BranchId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public DayOfWeek WeekDay { get; set; }

    public ScheduleRequestDTO(Guid branchId, DateTime startTime, DateTime endTime, DayOfWeek dayOfWeek)
    {
        BranchId = branchId;
        StartTime = startTime;
        EndTime = endTime;
        WeekDay = dayOfWeek;
    }
}
