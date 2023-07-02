using Domain.ValueObjects.Addresses;

namespace Domain.ValueObjects.DTO.Barber;

internal class BranchDTO
{
    public string Identifier { get; set; }
    public Address Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }

    public BranchDTO(string identifier, Address address, string phone, string email) {
        Identifier = identifier;
        Address = address;
        Phone = phone;
        Email = email;
    }
}
