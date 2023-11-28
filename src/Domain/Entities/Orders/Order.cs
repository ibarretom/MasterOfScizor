using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.ValueObjects.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Orders;

internal class Order : OrderBase
{

    [Required]
    [Column("status")]
    public OrderStatus Status { get; private set; }

    [Required]
    [Column("schedule_time")]
    public DateTime ScheduleTime { get; private init; }

    [Required]
    [Column("relocated_schedule")]
    public DateTime RelocatedSchedule { get; set; }

    [Required]
    public List<OrderJob> OrderServices { get; }

    [Required]
    [Column("total")]
    public decimal Total { get; private set; }

    private Order() : base() { }
    public Order(Branch branch, Employee worker, List<Service> services, OrderStatus orderStatus, DateTime scheduleTime)
          : base(branch, worker, services)
    {
        Status = orderStatus;
        ScheduleTime = scheduleTime;
        RelocatedSchedule = scheduleTime;
        Total = GetTotal();
        OrderServices = MapServices(services);
    }

    public decimal GetTotal()
    {
        return Services.Sum(service => !service.IsPromotionActive ? service.Price : service.PromotionalPrice);
    }

    public void UpdateStatus(OrderStatus orderStatus)
    {
        Status = orderStatus;
    }

    private List<OrderJob> MapServices(List<Service> services)
    {
        return services.Select(service => new OrderJob(service.Category, service.Name, service.Price, service.PromotionalPrice, service.IsPromotionActive, service.Duration, this, service )).ToList();
    }
}
