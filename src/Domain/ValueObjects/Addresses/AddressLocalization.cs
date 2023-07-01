namespace Domain.ValueObjects.Addresses;

internal class AddressLocalization
{
    public string Country { get; }
    public string State { get; }
    public string City { get; }
    public string Neighborhood { get; }
    public string Street { get; }
    public string ZipCode { get; }
    public GeoLocalization GeoLocalization { get; }

    public AddressLocalization(string country, string state, string city, string neighborhood, string street, string zipCode, GeoLocalization geoLocalization)
    {
        Country = country;
        State = state;
        City = city;
        Neighborhood = neighborhood;
        Street = street;
        ZipCode = zipCode;
        GeoLocalization = geoLocalization;
    }
}
