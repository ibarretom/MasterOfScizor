using Domain.Entities.Barbers.Service;

namespace Domain.ValueObjects.DTO.Barber;

internal class ServiceUpdateRequestDTO : ServiceRequestDTO
{
    public Guid Id { get; set; }

    public ServiceUpdateRequestDTO(Guid id, Guid branchId, Category? category, string name, string description, decimal price, decimal promotionalPrice, bool isPromotionActive, bool active, TimeSpan duration) : base(branchId, category, name, description, price, promotionalPrice, isPromotionActive, active, duration)
    {
        Id = id;
    }
}
