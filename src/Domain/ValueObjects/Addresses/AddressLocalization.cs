using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.ValueObjects.Addresses;

internal class AddressLocalization
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; init; }

    [Required]
    [Column("country")]
    public string Country { get; init; }

    [Required]
    [Column("state")]
    public string State { get; init; }

    [Required]
    [Column("city")]
    public string City { get; init; }

    [Required]
    [Column("neighborhood")]
    public string Neighborhood { get; init; }

    [Required]
    [Column("street")]
    public string Street { get; init; }

    [Required]
    [Column("zip_code")]
    public string ZipCode { get; init; }

    public AddressLocalization(string country, string state, string city, string neighborhood, string street, string zipCode)
    {
        Country = country;
        State = state;
        City = city;
        Neighborhood = neighborhood;
        Street = street;
        ZipCode = zipCode;
    }
}
