using System.ComponentModel;

namespace Domain.ValueObjects.Enums;

internal enum OrderStatus
{
    [Description("PENDING")]
    Pending,
    [Description("ACCEPTED")]
    Accepted,
    [Description("REJECTED")]
    Rejected,
    [Description("CANCELED")]
    Canceled,
    [Description("FINISHED")]
    Finished
}
