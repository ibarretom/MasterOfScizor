namespace Domain.ValueObjects.DTO;

internal class AddressLocalizationCreateDTO
{
    public string Country { get; }
    public string State { get; }
    public string City { get; }
    public string Neighborhood { get; }
    public string Street { get; }
    public string ZipCode { get; }

    public AddressLocalizationCreateDTO(string country, string state, string city, string neighborhood, string street, string zipCode)
    {
        Country = country;
        State = state;
        City = city;
        Neighborhood = neighborhood;
        Street = street;
        ZipCode = zipCode;
    }
}
