using Domain.Exceptions;
using Domain.Exceptions.Messages;

namespace Domain.Entities.Barbers;

internal class Schedule
{
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public DayOfWeek WeekDay { get; private set; }
    public DayOfWeek? OverflowingDay { get; private set; }

    public Schedule(TimeOnly startTime, TimeOnly endTime, DayOfWeek dayOfWeek)
    {
        WeekDay = dayOfWeek;
        SetScheduleTime(startTime, endTime);
    }

    private void SetScheduleTime(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime == endTime)
            throw new ScheduleException(ScheduleExceptionMessagesResource.START_TIME_AND_END_TIME_MUST_BE_DIFFERENT);

        StartTime = startTime;
        EndTime = endTime;

        if (startTime > endTime)
            OverflowingDay = (DayOfWeek?)(((int) WeekDay + 1) % 7);
    }

    public bool Includes(Schedule schedule)
    {
        if (!WeekDaysMatches(schedule))
            return false;
        
        var aDateToBeCompared = DateTime.UtcNow;
        
        var daysUntilThisSchedule = CalculateDaysUntilScheduleDayOfWeek(aDateToBeCompared);

        aDateToBeCompared = aDateToBeCompared.AddDays(daysUntilThisSchedule);

        var thisDate = GetScheduleDateTime(this, aDateToBeCompared);

        var dayThatMatchDesiredSchedule = schedule.WeekDay == thisDate.StartTime.DayOfWeek ? thisDate.StartTime : thisDate.EndTime;

        var compareDate = GetScheduleDateTime(schedule, dayThatMatchDesiredSchedule);

        var startTimeIsIncluded = thisDate.StartTime <= compareDate.StartTime && compareDate.StartTime <= thisDate.EndTime;
        var endTimeIsIncluded = thisDate.StartTime <= compareDate.EndTime && compareDate.EndTime <= thisDate.EndTime;

        return startTimeIsIncluded && endTimeIsIncluded;
    }

    private bool WeekDaysMatches(Schedule schedule)
    {
        if (schedule.OverflowingDay.HasValue)
            return WeekDay == schedule.WeekDay && OverflowingDay == schedule.OverflowingDay;

        if (OverflowingDay.HasValue)
            return WeekDay == schedule.WeekDay || OverflowingDay == schedule.WeekDay;

        return WeekDay == schedule.WeekDay;
    }

    public bool Includes(DateTime day)
    {
        if (!WeekDaysMatches(day))
            return false;

        var bestDateTimeForThisSchedule = GetDateToBeReference(day);

        var thisSchedule = GetScheduleDateTime(this, bestDateTimeForThisSchedule);

        var isAfterStartTime = DateTime.Compare(thisSchedule.StartTime, day) <= 0;
        var isBeforeStartTime = DateTime.Compare(thisSchedule.EndTime, day) >= 0;

        return isAfterStartTime && isBeforeStartTime;
    }

    private bool WeekDaysMatches(DateTime day)
    {
        if(OverflowingDay.HasValue)
            return WeekDay == day.DayOfWeek || OverflowingDay == day.DayOfWeek;

        return WeekDay == day.DayOfWeek;
    }

    public DateTime GetDateToBeReference(DateTime day)
    {
        return day.DayOfWeek == OverflowingDay ? day.AddDays(-1) : day;
    }

    public static (DateTime StartTime, DateTime EndTime) GetScheduleDateTime(Schedule desiredTimes, DateTime aDateToGetDaysForReference)
    {
        if(aDateToGetDaysForReference.DayOfWeek != desiredTimes.WeekDay)
            throw new ScheduleException(ScheduleExceptionMessagesResource.INVALID_DATE_FOR_THIS_SCHEDULE);

        var startTime = new DateTime(aDateToGetDaysForReference.Year, aDateToGetDaysForReference.Month, aDateToGetDaysForReference.Day, desiredTimes.StartTime.Hour, desiredTimes.StartTime.Minute, 0, aDateToGetDaysForReference.Kind);

        var endTime = new DateTime(aDateToGetDaysForReference.Year, aDateToGetDaysForReference.Month, aDateToGetDaysForReference.Day, desiredTimes.EndTime.Hour, desiredTimes.EndTime.Minute, 0, aDateToGetDaysForReference.Kind);

        if (desiredTimes.StartTime > desiredTimes.EndTime)
            endTime = endTime.AddDays(1);

        return (startTime, endTime);
    }

    public bool Includes(DayOfWeek dayOfWeek)
    {
        return WeekDaysMatches(dayOfWeek);
    }

    private bool WeekDaysMatches(DayOfWeek dayOfWeek)
    {
        if (OverflowingDay.HasValue)
            return WeekDay == dayOfWeek || OverflowingDay == dayOfWeek;

        return WeekDay == dayOfWeek;
    }

    public int CalculateDaysUntilScheduleDayOfWeek(DateTime day)
    {
        return (WeekDay - day.DayOfWeek + 7) % 7;
    }
    public override int GetHashCode()
    {
        return WeekDay.GetHashCode() + OverflowingDay.GetHashCode();
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
