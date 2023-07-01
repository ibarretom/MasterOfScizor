namespace Domain.ValueObjects.Addresses;

internal class AddressIdentifier
{
    public string Number { get; set; }
    public string Complement { get; set; }

    public AddressIdentifier(string number, string complement)
    {
        Number = number;
        Complement = complement;
    }
}
