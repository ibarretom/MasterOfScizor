using Domain.ValueObjects.DTO.Barber;

namespace DomainTest.ValueObjects.DTO;

internal class ServiceUpdateRequestDTOBuilder
{
    public static ServiceUpdateRequestDTO Build()
    {
        var serviceRequest = ServiceRequestDTOBuilder.Build();
        return new ServiceUpdateRequestDTO(Guid.NewGuid(), serviceRequest.BranchId, serviceRequest.Category, serviceRequest.Name, 
                serviceRequest.Description, serviceRequest.Price, serviceRequest.PromotionalPrice, 
                serviceRequest.IsPromotionActive, serviceRequest.Active, serviceRequest.Duration);
    }
}
