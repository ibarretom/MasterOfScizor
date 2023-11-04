namespace Domain.ValueObjects.DTO.Barber;

internal class LunchIntervalDTO : ScheduleRequestDTO
{
    public Guid EmployeeId { get; set; }

    public LunchIntervalDTO(TimeOnly startTime, TimeOnly endTime, DayOfWeek dayOfWeek, Guid branchId, Guid employeeId) : base(branchId, startTime, endTime, dayOfWeek)
    {
        EmployeeId = employeeId;
    }
}
