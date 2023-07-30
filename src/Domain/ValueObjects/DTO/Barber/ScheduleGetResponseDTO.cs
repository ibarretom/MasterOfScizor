using Domain.Entities.Barbers;

namespace Domain.ValueObjects.DTO.Barber;

internal class ScheduleGetResponseDTO
{
    public HashSet<Schedule> Items { get; }

    public ScheduleGetResponseDTO(HashSet<Schedule> schedule)
    {
        Items = schedule;
    }
}
