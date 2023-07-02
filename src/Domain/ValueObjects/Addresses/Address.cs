namespace Domain.ValueObjects.Addresses;

internal class Address
{
    public Guid AddressId { get; set; }
    public AddressIdentifier Identifier { get; set; }

    public Address(Guid addressId, AddressIdentifier identifier)
    {
        AddressId = addressId;
        Identifier = identifier;
    }
}
