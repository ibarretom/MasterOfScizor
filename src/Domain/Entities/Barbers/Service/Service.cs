using Domain.Services.Encription;
using System.Security.Cryptography;
using System.Text.Json;

namespace Domain.Entities.Barbers.Service;

internal class Service
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; }
    public Category Category { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal PromotionalPrice { get; set; }
    public bool IsPromotionActive { get; set; }
    public bool Active { get; set; }
    public string Worker { get; set; }
    public TimeSpan Duration { get; set; }

    public Service(Guid companyId, Category category, string name, string description, string price, string promotionalPrice, bool isPromotionActive, bool active, string worker, TimeSpan duration)
    {
        SetId();
        CompanyId = companyId;
        Category = category;
        Name = name;
        Description = description;
        Price = decimal.Parse(price);
        PromotionalPrice = decimal.Parse(promotionalPrice);
        IsPromotionActive = isPromotionActive;
        Active = active;
        Worker = worker;
        Duration = duration;
    }

    public void SetId()
    {
        if (Id == Guid.Empty)
            Id = Guid.NewGuid();
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        var jsonObjToCompare = JsonSerializer.Serialize(obj);
        var jsonObj = JsonSerializer.Serialize(this);

        return HashBuilder.Build(jsonObjToCompare) == HashBuilder.Build(jsonObj);
    }
}
