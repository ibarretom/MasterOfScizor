using Domain.Services.Encription;
using System.Text.Json;

namespace Domain.Entities;

internal class Employee : User
{
    public string CompanyId { get; }
    public bool Active { get; set; }
    public string Avatar { get; set; }
    public string Document { get; set; }

    public Employee(string companyId, bool active, string avatar, string document, string name, string email, string phone) : base(name, email, phone)
    {
        CompanyId = companyId;
        Active = active;
        Avatar = avatar;
        Document = document;
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
