using Domain.Entities.Barbers;
using Domain.ValueObjects.DTO.Barber;

namespace DomainTest.ValueObjects.DTO;

internal class ScheduleUpdateRequestDTOBuilder
{
    public static ScheduleUpdateRequestDTO Build()
    {
        var barnchId = Guid.NewGuid();
        var schedule = new HashSet<ScheduleRequestDTO>
        {
            new ScheduleRequestDTO(barnchId, new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute), new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute).AddHours(1), DayOfWeek.Monday),
            new ScheduleRequestDTO(barnchId, new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute), new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute).AddHours(1), DayOfWeek.Tuesday),
            new ScheduleRequestDTO(barnchId, new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute), new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute).AddHours(1), DayOfWeek.Wednesday),
            new ScheduleRequestDTO(barnchId, new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute), new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute).AddHours(1), DayOfWeek.Thursday),
            new ScheduleRequestDTO(barnchId, new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute), new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute).AddHours(1), DayOfWeek.Friday),
            new ScheduleRequestDTO(barnchId, new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute), new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute).AddHours(1), DayOfWeek.Saturday),
        };

        return new ScheduleUpdateRequestDTO(schedule);
    }
}
