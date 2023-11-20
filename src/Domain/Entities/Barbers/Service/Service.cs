using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Encription;
using System.Text.Json;

namespace Domain.Entities.Barbers.Service;

internal class Service
{
    public Guid Id { get; private set; } 
    public Category? Category { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; private set; }
    public decimal PromotionalPrice { get; private set; }
    public bool IsPromotionActive { get; set; }
    public bool Active { get; set; }
    public TimeSpan Duration { get; set; }

    public Service(Category? category, string name, string description, decimal price, decimal promotionalPrice, bool isPromotionActive, bool active, TimeSpan duration)
    {
        SetId();
        Category = category;
        Name = name;
        Description = description;
        SetPrices(price, promotionalPrice);
        IsPromotionActive = isPromotionActive;
        Active = active;
        Duration = duration;
    }

    public void SetId()
    {
        if (Id == Guid.Empty)
            Id = Guid.NewGuid();
    }

    public void SetPrices(decimal regularPrice, decimal promotionalPrice)
    {
        if (Decimal.Compare(regularPrice, 0) <= Decimal.Zero|| Decimal.Compare(promotionalPrice, 0) <= Decimal.Zero) 
            throw new ServiceException(ServiceExceptionMessagesResource.PRICE_MUST_BE_GRATHER_THEN_ZERO);

        if(Decimal.Compare(regularPrice, promotionalPrice) == Decimal.MinusOne) 
            throw new ServiceException(ServiceExceptionMessagesResource.PROMOTIONAL_PRICE_CANT_BE_GREATER_THEN_REGULAR_PRICE);

        Price = regularPrice;
        PromotionalPrice = promotionalPrice;
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
