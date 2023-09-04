using Domain.Entities.Barbers;

namespace Domain.ValueObjects.DTO.Barber;

internal class ScheduleCreateRequestDTO
{
    public HashSet<ScheduleRequestDTO> Schedule { get; set; }

    public ScheduleCreateRequestDTO(HashSet<ScheduleRequestDTO> schedule)
    {
        Schedule = schedule;
    }
}
