using Domain.Services.Encription;
using System.ComponentModel.Design;
using System.Text.Json;

namespace Domain.Entities.Barbers.Service;

internal class Category
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; }
    public string Name { get; set; }

    public Category()
    {
        Name = string.Empty;
    }
    public Category(string name, Guid companyId)
    {
        SetId();
        Name = name;
        CompanyId = companyId;
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
