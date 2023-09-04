using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Encription;
using System.Text.Json;

namespace Domain.Entities.Barbers;

internal class Schedule
{
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public DayOfWeek WeekDay { get; set; }

    public Schedule(DateTime startTime, DateTime endTime, DayOfWeek dayOfWeek)
    {
        SetScheduleTime(startTime, endTime);
        WeekDay = dayOfWeek;
    }

    public void SetScheduleTime(DateTime startTime, DateTime endTime)
    {
        if (DateTime.Compare(startTime, DateTime.MinValue) <= 0 || DateTime.Compare(endTime, DateTime.MinValue) <= 0) 
            throw new ScheduleException(ScheduleExceptionMessagesResource.INVALID_DATETIME);

        if (DateTime.Compare(startTime, endTime) >= 0) 
            throw new ScheduleException(ScheduleExceptionMessagesResource.START_TIME_MUST_BE_GREATER_THEN_END_TIME); 
        
        StartTime = startTime;
        EndTime = endTime;
    }

    public override int GetHashCode()
    {
        return WeekDay.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        var jsonObjToCompare = JsonSerializer.Serialize(obj);
        var jsonObj = JsonSerializer.Serialize(this);

        return HashBuilder.Build(jsonObjToCompare) == HashBuilder.Build(jsonObj);
    }
}
