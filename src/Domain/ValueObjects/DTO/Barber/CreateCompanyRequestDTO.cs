namespace Domain.ValueObjects.DTO.Barber;

internal class CreateCompanyRequestDTO
{
    public Guid OwnerId { get; set; }
    public string Identifier { get; set; }
    public string Name { get; set; }
    public BranchDTO Branch { get; set; }

    public CreateCompanyRequestDTO(Guid ownerId, string identifier, string name, BranchDTO branch)
    {
        OwnerId = ownerId;
        Identifier = identifier;
        Name = name;
        Branch = branch;
    }
}
