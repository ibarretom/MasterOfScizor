namespace Domain.ValueObjects.Addresses;

internal class Address
{
    public AddressLocalization Localization { get; }
    public AddressIdentifier Identifier { get; set; }

    public Address(AddressLocalization localization, AddressIdentifier identifier)
    {
        Localization = localization;
        Identifier = identifier;
    }
}
