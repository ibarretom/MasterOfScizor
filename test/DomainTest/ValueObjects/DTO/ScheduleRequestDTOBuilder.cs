using Domain.Entities.Barbers;
using Domain.ValueObjects.DTO.Barber;

namespace DomainTest.ValueObjects.DTO;

internal class ScheduleRequestDTOBuilder
{
    public static ScheduleCreateRequestDTO Build()
    {
        var barnchId = Guid.NewGuid();
        var schedule = new HashSet<Schedule>
        {
            new Schedule(barnchId, DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Monday),
            new Schedule(barnchId, DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Tuesday),
            new Schedule(barnchId, DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Wednesday),
            new Schedule(barnchId, DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Thursday),
            new Schedule(barnchId, DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Friday),
            new Schedule(barnchId, DateTime.Now, DateTime.Now.AddHours(1), DayOfWeek.Saturday),
        };

        return new ScheduleCreateRequestDTO(schedule);
    }
}
