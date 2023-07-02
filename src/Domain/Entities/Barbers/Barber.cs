using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Application")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Domain.Entities.Barbers;

internal class Barber
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; }
    public string Identifier { get; set; }
    public string Avatar { get; set; }
    public HashSet<Branch> Branch { get; } = new HashSet<Branch>();

    public Barber(Guid ownerId, string name, string identifier, string avatar)
    {
        SetId();
        OwnerId = ownerId;
        Name = name;
        Identifier = identifier;
        Avatar = avatar;
    }

    public void SetId()
    {
        if(Id == Guid.Empty)
            Id = Guid.NewGuid();
    }

    public void AddBranch(Branch branch)
    {
        Branch.Add(branch);
    }

    public void RemoveBranch(Branch branch)
    {
        Branch.RemoveWhere(existentBranch => branch.Identifier == existentBranch.Identifier);
    }
}
