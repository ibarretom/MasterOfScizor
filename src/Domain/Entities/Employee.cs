using Domain.Entities.Barbers;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Encription;
using System.Text.Json;

namespace Domain.Entities;

internal class Employee : User
{
    public Guid BranchId { get; private set; }
    public bool Active { get; set; }
    public string Avatar { get; set; }
    public string Document { get; set; }
    public Schedule? LunchInterval { get; set; }
    public Employee(Guid branchId, bool active, string avatar, string document, string name, string email, string phone) : base(name, email, phone)
    {
        SetBranchId(branchId);
        Active = active;
        Avatar = avatar;
        Document = document;
    } 
    
    public Employee(Guid branchId, bool active, string avatar, string document, string name, string email, string phone, Schedule lunchInterval) : base(name, email, phone)
    {
        SetBranchId(branchId);
        Active = active;
        Avatar = avatar;
        Document = document;
        LunchInterval = lunchInterval;
    }

    private void SetBranchId(Guid branchId)
    {
        if (branchId == Guid.Empty)
            throw new UserException(CompanyExceptionMessagesResource.INVALID_BRANCH_ID);

        BranchId = branchId;
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
