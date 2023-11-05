using Domain.Services.Authentication;
using Domain.ValueObjects.Enums;

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

    public void AddRoles(HashSet<UserRole> roles)
    {
        Roles.UnionWith(roles);
    }

    public void SetPassword(string password, IEncryptService encryptService)
    {
        Password = encryptService.Hash(GetPasswordToBeHashed(this, password));
    }

    public static string GetPasswordToBeHashed(User user, string password)
    {
        return $"{user.Email}{password}";
    }
}
