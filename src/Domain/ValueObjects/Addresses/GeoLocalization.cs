namespace Domain.ValueObjects.Addresses;

internal class GeoLocalization
{
    public string Latitude { get; }
    public string Longitude { get; }

    public GeoLocalization(string latitude, string longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
}
