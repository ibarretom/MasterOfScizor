using Domain.ValueObjects.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Barbers;

internal class Configuration
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; private set; }

    [Column("queue_type")]
    public OrderQueueType OrderQueueType { get; set; }

    [Column("queue_according_to_virtual_queue")]
    public bool QueueAccordingToVirtualQueue { get; set; }

    [Column("queue_limit")]
    public int? QueueLimit { get; set; }

    [Column("schedule_delay_time")]
    public TimeSpan ScheduleDelayTime { get; set; } = TimeSpan.FromMinutes(15);

    [Column("auto_adjust_schedule_time")]
    public bool AutoAdjustScheduleTime { get; set; }

    [Column("schedule_default_interval")]
    public TimeSpan ScheduleDefaultInterval { get; set; } = TimeSpan.FromMinutes(30);

    public Branch Branch { get; init; }

    public Configuration(OrderQueueType orderQueueType, bool queueAccordingToVirtualQueue, int? queueLimit, TimeSpan scheduleDelayTime, bool autoAdjustScheduleTime)
    {
        OrderQueueType = orderQueueType;
        QueueAccordingToVirtualQueue = queueAccordingToVirtualQueue;
        QueueLimit = queueLimit;
        ScheduleDelayTime = scheduleDelayTime;
        AutoAdjustScheduleTime = autoAdjustScheduleTime;
    }
}
