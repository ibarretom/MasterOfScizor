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
        var afterMidnight = new DateTime(now.Year, now.AddDays(1).Month, now.AddDays(1).Day, 1, 0, 0);

        var schedule = new Schedule(new TimeOnly(beforeMidnight.Hour, beforeMidnight.Minute), new TimeOnly(afterMidnight.AddHours(1).Hour, afterMidnight.Minute), beforeMidnight.DayOfWeek);

        var scheduleToCompare = new Schedule(new TimeOnly(afterMidnight.AddHours(0.5).Hour, afterMidnight.Minute),
                                            new TimeOnly(afterMidnight.AddHours(1).Hour, afterMidnight.Minute), afterMidnight.DayOfWeek);

        Assert.True(schedule.Includes(scheduleToCompare));
    }

    [Fact]
    public void ShouldIncludeInSaturdayToSunday()
    {
        var now = DateTime.UtcNow;
        now = now.AddDays((DayOfWeek.Saturday - now.DayOfWeek - +7) % 7);

        var beforeMidnight = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);
        var afterMidnight = new DateTime(now.Year, now.Month, now.AddDays(1).Day, 1, 0, 0);

        var schedule = new Schedule(new TimeOnly(beforeMidnight.Hour, beforeMidnight.Minute), new TimeOnly(afterMidnight.AddHours(1).Hour, afterMidnight.Minute), DayOfWeek.Saturday);

        var scheduleToCompare = new Schedule(new TimeOnly(afterMidnight.AddHours(0.5).Hour, afterMidnight.Minute),
                                            new TimeOnly(afterMidnight.AddHours(1).Hour, afterMidnight.Minute), DayOfWeek.Sunday);

        Assert.True(schedule.Includes(scheduleToCompare));
    }

    [Fact]
    public void ShouldNotIncludeWhenScheduleHasOverflowingDayAndTheDesiredDayIsScheduleForTheNextDayInTimeOfPreviousDayStartTime()
    {
        var now = DateTime.UtcNow;

        var beforeMidnight = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);
        var afterMidnight = new DateTime(now.AddDays(1).Year, now.AddDays(1).Month, now.AddDays(1).Day, 2, 0, 0);

        var schedule = new Schedule(new TimeOnly(beforeMidnight.Hour, beforeMidnight.Minute), new TimeOnly(afterMidnight.Hour, afterMidnight.Minute), beforeMidnight.DayOfWeek);

        var dayToCompare = new DateTime(now.AddDays(1).Year, now.AddDays(1).Month, now.AddDays(1).Day, 23, 30, 0);

        Assert.False(schedule.Includes(dayToCompare));
    }

    [Fact]
    public void ShouldThrowsWhenDateToCompareDayOfWeekIsDifferentThenScheduleDayOfWeekForGetScheduleDates()
    {
        var now = DateTime.UtcNow;

        var beforeMidnight = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);
        var afterMidnight = new DateTime(now.Year, now.Month, now.AddDays(1).Day, 2, 0, 0);

        var schedule = new Schedule(new TimeOnly(beforeMidnight.Hour, beforeMidnight.Minute), new TimeOnly(afterMidnight.Hour, afterMidnight.Minute), afterMidnight.DayOfWeek);

        var dayToCompare = new DateTime(now.Year, now.Month, now.AddDays(1).Day, 23, 30, 0);

        var exception = Assert.Throws<ScheduleException>(() => Schedule.GetScheduleDateTime(schedule, now));

        Assert.True(exception.Message.Equals(ScheduleExceptionMessagesResource.INVALID_DATE_FOR_THIS_SCHEDULE));
    }

    [Fact]
    public void ShouldReturnFalseWhenSchedulesDayOfWeekDoesNotMatches()
    {
        var now = DateTime.UtcNow;

        var schedule = new Schedule(new TimeOnly(20, now.Minute), new TimeOnly(23, now.Minute), DayOfWeek.Saturday);

        var scheduleToCompare = new Schedule(new TimeOnly(21, now.Minute), new TimeOnly(22, now.Minute), DayOfWeek.Sunday);

        Assert.False(schedule.Includes(scheduleToCompare));
    }

    [Fact]
    public void ShouldReturnFalseWhenSchedulesWithOverflowingIsNotMatching()
    {
        var now = DateTime.UtcNow;

        var schedule = new Schedule(new TimeOnly(20, now.Minute), new TimeOnly(2, now.Minute), DayOfWeek.Saturday);

        var scheduleToCompare = new Schedule(new TimeOnly(21, now.Minute), new TimeOnly(2, now.Minute), DayOfWeek.Sunday);

        Assert.False(schedule.Includes(scheduleToCompare));
    }

    [Fact]
    public void ShouldReturnFalseWhenScheduleWithOverflowingIsNotMatching()
    {
        var now = DateTime.UtcNow;

        var schedule = new Schedule(new TimeOnly(20, now.Minute), new TimeOnly(2, now.Minute), DayOfWeek.Saturday);

        var scheduleToCompare = new Schedule(new TimeOnly(21, now.Minute), new TimeOnly(22, now.Minute), DayOfWeek.Sunday);

        Assert.False(schedule.Includes(scheduleToCompare));
    }

    [Fact]
    public void ShouldIncludeADayThatDayOfWeekendMatches()
    {
        var now = DateTime.UtcNow;

        var schedule = new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.AddHours(2).Hour, now.AddHours(2).Minute), now.DayOfWeek);

        var date = now.AddHours(1);

        Assert.True(schedule.Includes(date.DayOfWeek));
    }

    [Fact]
    public void ShouldIncludeWhenOverflowingDayMatches()
    {
        var now = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 23, 0, 0);

        var schedule = new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.AddHours(2).Hour, now.AddHours(2).Minute), now.DayOfWeek);

        var date = now.AddHours(1);

        Assert.True(schedule.Includes(date.DayOfWeek));
    }

    [Fact]
    public void ShouldGiveADayToBeReferenceBasedOnTheStartingTimeOfTheSchedule()
    {
        var now = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 23, 0, 0);

        var scheduleWithOverflow = new Schedule(new TimeOnly(now.Hour, now.Minute), new TimeOnly(now.AddHours(2).Hour, now.AddHours(2).Minute), now.DayOfWeek);

        var dateMatchingStartTime = scheduleWithOverflow.GetDateToBeReference(now);

        Assert.Equal(dateMatchingStartTime, now);

        var dateMatchingOverflowingDay = scheduleWithOverflow.GetDateToBeReference(now.AddDays(1));

        Assert.Equal(dateMatchingOverflowingDay, now);
    }
}
