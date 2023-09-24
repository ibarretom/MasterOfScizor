using Domain.Entities.Barbers;
using Domain.Exceptions;
using Domain.Exceptions.Messages;

namespace DomainTest.Entities.Barbers;

public class ScheduleTest
{
    [Fact]
    public void ShouldNotBeAbleToCreateAScheduleWithEndTimeEqualsToStartTime()
    {
        var now = DateTime.UtcNow;
        var timeOnly = new TimeOnly(DateTime.UtcNow.Hour, DateTime.UtcNow.Minute);

        var exception = Assert.Throws<ScheduleException>(() => new Schedule(timeOnly, timeOnly, DayOfWeek.Monday));

        Assert.True(exception.Message.Equals(ScheduleExceptionMessagesResource.START_TIME_AND_END_TIME_MUST_BE_DIFFERENT));
    }

    [Fact]
    public void ShouldReturnFalseForIncludeInThisValues()
    {
        var now = DateTime.UtcNow;

        var beforeMidnight = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);
        var afterMidnight = new DateTime(now.Year, now.Month, now.AddDays(1).Day, 1, 0, 0);

        var schedule = new Schedule(new TimeOnly(beforeMidnight.Hour, beforeMidnight.Minute), new TimeOnly(afterMidnight.Hour + 1, afterMidnight.Minute), beforeMidnight.DayOfWeek);

        var scheduleToCompare = new Schedule(new TimeOnly(afterMidnight.AddHours(2).Hour, afterMidnight.Minute), 
                                            new TimeOnly(afterMidnight.AddHours(3).Hour, afterMidnight.Minute), afterMidnight.DayOfWeek);

        Assert.False(schedule.Includes(scheduleToCompare));
    }

    [Fact]
    public void ThisValuesShouldBeIncludedInTimeOnly()
    {
        var now = DateTime.UtcNow;

        var beforeMidnight = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);
        var afterMidnight = new DateTime(now.Year, now.Month, now.AddDays(1).Day, 1, 0, 0);

        var schedule = new Schedule(new TimeOnly(beforeMidnight.Hour, beforeMidnight.Minute), new TimeOnly(afterMidnight.AddHours(1).Hour, afterMidnight.Minute), beforeMidnight.DayOfWeek);

        var scheduleToCompare = new Schedule(new TimeOnly(afterMidnight.AddHours(0.5).Hour, afterMidnight.Minute),
                                            new TimeOnly(afterMidnight.AddHours(1).Hour, afterMidnight.Minute), afterMidnight.DayOfWeek);

        Assert.True(schedule.Includes(scheduleToCompare));
    }
    
    [Fact]
    public void ShouldIncludeInSaturdayToSunday()
    {
        var now = DateTime.UtcNow;

        var beforeMidnight = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);
        var afterMidnight = new DateTime(now.Year, now.Month, now.AddDays(1).Day, 1, 0, 0);

        var schedule = new Schedule(new TimeOnly(beforeMidnight.Hour, beforeMidnight.Minute), new TimeOnly(afterMidnight.AddHours(1).Hour, afterMidnight.Minute), DayOfWeek.Saturday);

        var scheduleToCompare = new Schedule(new TimeOnly(afterMidnight.AddHours(0.5).Hour, afterMidnight.Minute),
                                            new TimeOnly(afterMidnight.AddHours(1).Hour, afterMidnight.Minute), DayOfWeek.Sunday);

        Assert.True(schedule.Includes(scheduleToCompare));
    }
}
