using Domain.Entities.Barbers;

namespace Domain.ValueObjects.DTO.Barber;

internal class ScheduleUpdateRequestDTO : ScheduleCreateRequestDTO
{
    public ScheduleUpdateRequestDTO(HashSet<Schedule> schedule) : base(schedule)
    {
    }
}
