using Domain.Entities.Barbers;
using Domain.Entities.Barbers.Service;
using Domain.Entities.Orders;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Encription;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Domain.Entities;

internal class Employee : User
{
    public Branch Branch { get; private set; }

    [Column("active")]
    public bool Active { get; set; }

    [Column("avatar")]
    public string Avatar { get; set; }

    [Column("Document")]
    public string Document { get; set; }

    public HashSet<Schedule> LunchInterval { get; private set; } = new HashSet<Schedule>();
    
    public HashSet<Service> Services { get; private set; } = new HashSet<Service>();
    
    public List<Order> Orders { get; } = new List<Order>();
    private Employee() : base() { }
    public Employee(Branch branch, bool active, string avatar, string document, string name, string email, string phone) : base(name, email, phone)
    {
        Branch = branch;
        Active = active;
        Avatar = avatar;
        Document = document;
    } 

    public void AddLunchInterval(Schedule schedule)
    {
        LunchInterval.Add(schedule);
    }
    
    public void AddLunchInterval(HashSet<Schedule> schedules)
    {
        LunchInterval.UnionWith(schedules);
    }

    public void AddServices(List<Service> services)
    {
        Services = Services.Concat(services).ToHashSet();
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
