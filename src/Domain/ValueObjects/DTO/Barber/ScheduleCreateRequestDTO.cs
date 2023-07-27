using Domain.Entities.Barbers;
using System.Collections.Generic;

namespace Domain.ValueObjects.DTO.Barber;

internal class ScheduleCreateRequestDTO
{
    public HashSet<Schedule> Schedule { get; set; }

    public ScheduleCreateRequestDTO(HashSet<Schedule> schedule)
    {
        Schedule = schedule;
    }
}
