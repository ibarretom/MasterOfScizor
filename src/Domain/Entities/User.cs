using Domain.Services.Authentication;
using Domain.ValueObjects;

namespace Domain.Entities;

public class User
{
    public Guid Id { get; }
    public string Name { get; set; }
    public string Email { get; }
    public string Phone { get; set; }
    public string Password { get; private set; } = string.Empty;
    public HashSet<UserRole> Roles { get; private set; }
    public DateTime CreatedAt { get; }

    public User(string name, string email, string phone)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Phone = phone;
        CreatedAt = DateTime.Now;
        Roles = new HashSet<UserRole>();
    }

    public void AddRoles(UserRole role)
    {
        Roles.Add(role);
    }

    public void SetPassword(string password, IEncriptService encriptService)
    {
        Password = encriptService.Hash($"{Email}{password}");
    }
}
