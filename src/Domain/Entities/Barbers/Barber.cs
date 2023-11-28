using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Application")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Domain.Entities.Barbers;

internal class Barber
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; private set; }

    public User Owner { get; init;  }

    [Required]
    [Column("name")]
    public string Name { get; set; }

    [Column("identifier")]
    public string Identifier { get; }

    [Column("avatar")]
    public string Avatar { get; set; }

    public HashSet<Branch> Branch { get; } = new HashSet<Branch>();
    
    private Barber () { }
    
    public Barber(User owner, string name, string identifier, string avatar)
    {
        SetId();
        Owner = owner;
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
