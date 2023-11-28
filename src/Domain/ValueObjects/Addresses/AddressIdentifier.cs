using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.ValueObjects.Addresses;

internal class AddressIdentifier
{
    [Required]
    [Column("number")]
    public string Number { get; set; }

    [MaxLength(20)]
    [Column("complement")]
    public string Complement { get; set; }

    public Address Address { get; set; }

    public AddressIdentifier(string number, string complement)
    {
        Number = number;
        Complement = complement;
    }
}
