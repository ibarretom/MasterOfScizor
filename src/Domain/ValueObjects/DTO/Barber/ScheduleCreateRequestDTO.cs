using Domain.Entities.Barbers;

namespace Domain.ValueObjects.DTO.Barber;

internal class ScheduleCreateRequestDTO
{
    public HashSet<Schedule> Schedule { get; set; }

    public ScheduleCreateRequestDTO(HashSet<Schedule> schedule)
    {
        Schedule = schedule;
    }
}
