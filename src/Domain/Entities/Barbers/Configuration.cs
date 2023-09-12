using Domain.ValueObjects.Enums;

namespace Domain.Entities.Barbers;

internal class Configuration
{
    public OrderQueueType OrderQueueType { get; set; }
    public bool QueueAccordingToVirtualQueue { get; set; }
    public int? QueueLimit { get; set; }
    public TimeSpan ScheduleDelayTime { get; set; }
    public bool AutoAdjustScheduleTime { get; set; }
    public TimeSpan ScheduleDefaultInterval { get; set; } = TimeSpan.FromMinutes(30);

    public Configuration(OrderQueueType orderQueueType, bool queueAccordingToVirtualQueue, int? queueLimit, TimeSpan scheduleDelayTime, bool autoAdjustScheduleTime)
    {
        OrderQueueType = orderQueueType;
        QueueAccordingToVirtualQueue = queueAccordingToVirtualQueue;
        QueueLimit = queueLimit;
        ScheduleDelayTime = scheduleDelayTime;
        AutoAdjustScheduleTime = autoAdjustScheduleTime;
    }
}
