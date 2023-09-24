namespace Domain.ValueObjects.DTO.Barber;

internal class ScheduleRequestDTO
{
    public Guid BranchId { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public DayOfWeek WeekDay { get; set; }

    public ScheduleRequestDTO(Guid branchId, TimeOnly startTime, TimeOnly endTime, DayOfWeek dayOfWeek)
    {
        BranchId = branchId;
        StartTime = startTime;
        EndTime = endTime;
        WeekDay = dayOfWeek;
    }
}
