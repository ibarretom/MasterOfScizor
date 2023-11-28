using Domain.Entities;

namespace Domain.ValueObjects.DTO.Barber;

internal class CreateCompanyRequestDTO
{
    public User Owner { get; set; }
    public string Identifier { get; set; }
    public string Name { get; set; }
    public BranchDTO Branch { get; set; }

    public CreateCompanyRequestDTO(User owner, string identifier, string name, BranchDTO branch)
    {
        Owner = owner;
        Identifier = identifier;
        Name = name;
        Branch = branch;
    }
}
