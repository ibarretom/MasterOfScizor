using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Encription;
using System.Text.Json;

namespace Domain.Entities.Barbers;

internal class Schedule
{
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public DayOfWeek WeekDay { get; set; }

    public Schedule(TimeOnly startTime, TimeOnly endTime, DayOfWeek dayOfWeek)
    {
        SetScheduleTime(startTime, endTime);
        WeekDay = dayOfWeek;
    }

    public void SetScheduleTime(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime == endTime)
            throw new ScheduleException(ScheduleExceptionMessagesResource.START_TIME_AND_END_TIME_MUST_BE_DIFFERENT);

        StartTime = startTime;
        EndTime = endTime;
    }

    public bool Includes(Schedule schedule)
    {
        var thisDate = GetScheduleDateTime(this);

        var compareDate = GetScheduleDateTime(schedule);

        var startTimeIsIncluded = thisDate.StartTime <= compareDate.StartTime && compareDate.StartTime <= thisDate.EndTime;
        var endTimeIsIncluded = thisDate.StartTime <= compareDate.EndTime && compareDate.EndTime <= thisDate.EndTime;

        return startTimeIsIncluded && endTimeIsIncluded;
    }

    public bool Includes(DateTime day)
    {
        var thisSchedule = GetScheduleDateTime(this);

        var isAfterStartTime = DateTime.Compare(thisSchedule.StartTime, day) <= 0;
        var isBeforeStartTime = DateTime.Compare(thisSchedule.EndTime, day) >= 0;

        return isAfterStartTime && isBeforeStartTime;
    }

    public static (DateTime StartTime, DateTime EndTime) GetScheduleDateTime(Schedule desiredTimes)
    {
        var now = DateTime.Now.ToUniversalTime();
        
        var daysDifference = (desiredTimes.WeekDay - now.DayOfWeek + 7) % 7;

        var startTime = new DateTime(now.Year, now.Month, now.Day, desiredTimes.StartTime.Hour, desiredTimes.StartTime.Minute, 0);
        startTime = startTime.AddDays(daysDifference);

        var endTime = new DateTime(now.Year, now.Month, now.Day, desiredTimes.EndTime.Hour, desiredTimes.EndTime.Minute, 0);
        endTime = endTime.AddDays(daysDifference);

        if (desiredTimes.StartTime > desiredTimes.EndTime)
            endTime = endTime.AddDays(1);

        return (startTime, endTime);
    }

    public bool DayIncluded(DateTime day)
    {
        var thisSchedule = GetScheduleDateTime(this);

        return thisSchedule.StartTime.DayOfWeek == day.DayOfWeek || thisSchedule.EndTime.DayOfWeek == day.DayOfWeek;
    }

    public override int GetHashCode()
    {
        return WeekDay.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var otherSchedule = (Schedule)obj;

        return WeekDay == otherSchedule.WeekDay &&
               StartTime == otherSchedule.StartTime &&
               EndTime == otherSchedule.EndTime;
    }
}
