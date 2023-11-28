using Domain.Services.Encription;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using System.Text.Json;

namespace Domain.Entities.Barbers.Service;

internal class Category
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; private set; }

    [Column("name")]
    public string Name { get; set; }

    public HashSet<Service> Services { get; init;  } = new HashSet<Service>();
    public Branch Branch { get; init; }

    public Category()
    {
        Name = string.Empty;
    }
    public Category(string name)
    {
        SetId();
        Name = name;
    }

    public void SetId()
    {
        if (Id == Guid.Empty)
            Id = Guid.NewGuid();
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        var jsonObjToCompare = JsonSerializer.Serialize(obj);
        var jsonObj = JsonSerializer.Serialize(this);

        return HashBuilder.Build(jsonObjToCompare) == HashBuilder.Build(jsonObj);
    }
}
