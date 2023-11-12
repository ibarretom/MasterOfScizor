namespace Domain.ValueObjects.Barbers;

internal class ScheduleTimeStatus
{
    public DateTime Time { get; }
    public bool IsAvailable { get; }

    public ScheduleTimeStatus(DateTime time, bool isAvailable)
    {
        Time = time;
        IsAvailable = isAvailable;
    }

    public override int GetHashCode()
    {
        return Time.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ScheduleTimeStatus timeDisponibilty)
            return false;

        return Time == timeDisponibilty.Time;
    }
}
