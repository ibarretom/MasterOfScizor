using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.ValueObjects.Addresses;


internal class Address
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; private set; }

    [Required]
    public AddressLocalization Localization { get; }
    
    [Required]
    public AddressIdentifier Identifier { get; set; }

    private Address() { }
    public Address(AddressLocalization localization, AddressIdentifier identifier)
    {
        Localization = localization;
        Identifier = identifier;
    }
}
