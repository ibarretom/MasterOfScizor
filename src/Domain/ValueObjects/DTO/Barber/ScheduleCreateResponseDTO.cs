using Domain.Entities.Barbers;

namespace Domain.ValueObjects.DTO.Barber;

internal class ScheduleCreateResponseDTO
{
    public HashSet<Schedule> CreatedSchedule { get; set; } = new HashSet<Schedule> { };
    public HashSet<Schedule> ExistentSchedule { get; set; } = new HashSet<Schedule> { };

    public ScheduleCreateResponseDTO()
    {
    }
}
