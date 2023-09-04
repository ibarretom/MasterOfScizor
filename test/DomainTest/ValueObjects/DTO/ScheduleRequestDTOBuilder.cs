using Domain.Entities.Barbers;
using Domain.ValueObjects.DTO.Barber;

namespace DomainTest.ValueObjects.DTO;

internal class ScheduleRequestDTOBuilder
{
    public static ScheduleCreateRequestDTO Build()
    {
        var barnchId = Guid.NewGuid();
        var schedule = new HashSet<ScheduleRequestDTO>
        {
            new ScheduleRequestDTO(barnchId, DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Monday),
            new ScheduleRequestDTO(barnchId, DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Tuesday),
            new ScheduleRequestDTO(barnchId, DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Wednesday),
            new ScheduleRequestDTO(barnchId, DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Thursday),
            new ScheduleRequestDTO(barnchId, DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Friday),
            new ScheduleRequestDTO(barnchId, DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Saturday),
        };

        return new ScheduleCreateRequestDTO(schedule);
    }
}
