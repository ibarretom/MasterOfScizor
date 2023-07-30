using Domain.Entities.Barbers.Service;

namespace Domain.ValueObjects.DTO.Barber;

internal class ServiceGetResponseDTO
{
    public IEnumerable<Service> Items { get; }

    public ServiceGetResponseDTO(IEnumerable<Service> services)
    {
        Items = services;
    }
}
