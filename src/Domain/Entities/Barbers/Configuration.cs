using Domain.ValueObjects.Enums;

namespace Domain.Entities.Barbers;

internal class Configuration
{
    public OrderQueueType OrderQueueType { get; set; }
    public bool QueueAccordingToVirtualQueue { get; set; }
    public int? QueueLimit { get; set; }
    public Schedule LunchInterval { get; set; }
    public TimeSpan ScheduleDelayTime { get; set; }
    public bool AutoAdjustScheduleTime { get; set; }

    public Configuration(OrderQueueType orderQueueType, bool queueAccordingToVirtualQueue, int? queueLimit, Schedule lunchInterval, TimeSpan scheduleDelayTime, bool autoAdjustScheduleTime)
    {
        OrderQueueType = orderQueueType;
        QueueAccordingToVirtualQueue = queueAccordingToVirtualQueue;
        QueueLimit = queueLimit;
        LunchInterval = lunchInterval;
        ScheduleDelayTime = scheduleDelayTime;
        AutoAdjustScheduleTime = autoAdjustScheduleTime;
    }
}
