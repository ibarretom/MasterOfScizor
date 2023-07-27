using Domain.Services.Encription;
using System.Text.Json;

namespace Domain.Entities.Barbers;

internal class Schedule
{
    public Guid BranchId { get; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DayOfWeek WeekDay { get; set; }

    public Schedule(Guid branchId, DateTime startTime, DateTime endTime, DayOfWeek dayOfWeek)
    {
        BranchId = branchId;
        StartTime = startTime;
        EndTime = endTime;
        WeekDay = dayOfWeek;
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
