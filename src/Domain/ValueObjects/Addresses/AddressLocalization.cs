namespace Domain.ValueObjects.Addresses;

internal class AddressLocalization
{
    public Guid Id { get; private set; }
    public string Country { get; }
    public string State { get; }
    public string City { get; }
    public string Neighborhood { get; }
    public string Street { get; }
    public string ZipCode { get; }

    public AddressLocalization(string country, string state, string city, string neighborhood, string street, string zipCode)
    {
        SetId();
        Country = country;
        State = state;
        City = city;
        Neighborhood = neighborhood;
        Street = street;
        ZipCode = zipCode;
    }

    private void SetId()
    {
        if(Guid.Empty == Id)
            Id = Guid.NewGuid();
    }
}
