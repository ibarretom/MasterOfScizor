using System.ComponentModel;

namespace Domain.ValueObjects.Enums;

internal enum OrderQueueType
{
    [Description("QUEUE")]
    Queue,
    [Description("SCHEDULE")]
    Schedule
}
