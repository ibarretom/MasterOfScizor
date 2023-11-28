using Domain.Entities.Barbers.Service;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities.Orders;

[PrimaryKey(nameof(Id))]
internal class OrderJob
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; }

    public Category? Category { get; private set; }

    [Required]
    [Column("name")]
    public string Name { get; private set; }

    [Required]
    [Column("price")]
    public decimal Price { get; private set; }


    [Column("promotional_price")]
    public decimal PromotionalPrice { get; private set; }

    [Required]
    [Column("is_promotion_active")]
    public bool IsPromotionActive { get; private set; }

    [Required]
    [Column("duration")]
    public TimeSpan Duration { get; private set; }

    public Order Order { get; }

    public Service Service { get; }

    public OrderJob() { }
    public OrderJob(Category? category, string name, decimal price, decimal promotionalPrice, bool isPromotionActive, TimeSpan duration, Order order, Service service)
    {
        Category = category;
        Name = name;
        Price = price;
        PromotionalPrice = promotionalPrice;
        IsPromotionActive = isPromotionActive;
        Duration = duration;
        Order = order;
        Service = service;
    }
}
