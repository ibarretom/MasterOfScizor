using Domain.Entities.Barbers;
using Domain.Exceptions;
using Domain.Exceptions.Messages;

namespace DomainTest.Entities.Barbers;

public class ScheduleTest
{
    [Fact]
    public void ShouldNoBeAbleToCreateAScheduleWithEndTimeGreaterThenEndTime()
    {
        var now = DateTime.UtcNow;
        var exception = Assert.Throws<ScheduleException>(() => new Schedule(Guid.NewGuid(), now, now.AddHours(-1), DayOfWeek.Monday));

        Assert.True(exception.Message.Equals(ScheduleExceptionMessagesResource.START_TIME_MUST_BE_GREATER_THEN_END_TIME));
    }

    [Fact]
    public void ShouldNotBeAbleToCreateAScheduleWithEndTimeEqualsToStartTime()
    {
        var now = DateTime.UtcNow;
        var exception = Assert.Throws<ScheduleException>(() => new Schedule(Guid.NewGuid(), now, now, DayOfWeek.Monday));
        Assert.True(exception.Message.Equals(ScheduleExceptionMessagesResource.START_TIME_MUST_BE_GREATER_THEN_END_TIME));
    }

    [Fact]
    public void ShouldNotBeAbleToCreateAScheduleWithInvalidDateTime()
    {
        var exception = Assert.Throws<ScheduleException>(() => new Schedule(Guid.NewGuid(), DateTime.MinValue, DateTime.MinValue, DayOfWeek.Monday));
        Assert.True(exception.Message.Equals(ScheduleExceptionMessagesResource.INVALID_DATETIME));
    }
}
