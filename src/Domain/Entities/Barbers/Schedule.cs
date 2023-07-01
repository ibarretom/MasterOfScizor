using Domain.Services.Encription;
using System.Text.Json;

namespace Domain.Entities.Barbers;

internal class Schedule
{
    public Guid CompanyId { get; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DayOfWeek DayOfWeek { get; set; }

    public Schedule(Guid companyId, DateTime startTime, DateTime endTime, DayOfWeek dayOfWeek)
    {
        CompanyId = companyId;
        StartTime = startTime;
        EndTime = endTime;
        DayOfWeek = dayOfWeek;
    }

    public override int GetHashCode()
    {
        return DayOfWeek.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        var jsonObjToCompare = JsonSerializer.Serialize(obj);
        var jsonObj = JsonSerializer.Serialize(this);

        return HashBuilder.Build(jsonObjToCompare) == HashBuilder.Build(jsonObj);
    }
}
