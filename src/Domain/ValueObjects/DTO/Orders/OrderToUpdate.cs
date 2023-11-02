using Domain.ValueObjects.Enums;

namespace Domain.ValueObjects.DTO.Orders;

internal class OrderToUpdate
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
}
