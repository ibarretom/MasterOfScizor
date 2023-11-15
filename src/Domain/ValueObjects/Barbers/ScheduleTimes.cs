namespace Domain.ValueObjects.Barbers;

internal class ScheduleTimes
{
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }

    public ScheduleTimes(DateTime start, DateTime end)
    {
        if (start > end)
            throw new ArgumentException("Start time must be before end time");

        Start = start;
        End = end;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var scheduleTimes = (ScheduleTimes)obj;
        return Start.Equals(scheduleTimes.Start) && End.Equals(scheduleTimes.End);
    }
}

