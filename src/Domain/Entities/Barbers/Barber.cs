namespace Domain.Entities.Barbers;

internal class Barber
{
    public string Name { get; set; }
    public string Identifier { get; set; }
    public string Avatar { get; set; }
    public HashSet<Branch> Branch { get; } = new HashSet<Branch>();

    public Barber(string name, string identifier, string avatar)
    {
        Name = name;
        Identifier = identifier;
        Avatar = avatar;
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
