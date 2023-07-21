using Domain.Entities.Barbers.Service;

namespace Domain.ValueObjects.DTO.Barber;

internal class ServiceRequestDTO
{
    public Guid BranchId { get; set; }
    public Category? Category { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal PromotionalPrice { get; set; }
    public bool IsPromotionActive { get; set; }
    public bool Active { get; set; }
    public TimeSpan Duration { get; set; }

    public ServiceRequestDTO(Guid branchId, Category? category, string name, string description, decimal price, decimal promotionalPrice, bool isPromotionActive, bool active, TimeSpan duration)
    {
        BranchId = branchId;
        Category = category;
        Name = name;
        Description = description;
        Price = price;
        PromotionalPrice = promotionalPrice;
        IsPromotionActive = isPromotionActive;
        Active = active;
        Duration = duration;
    }
}
