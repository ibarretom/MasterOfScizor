using Domain.Services.Authentication;
using Domain.ValueObjects.Barbers;
using Domain.ValueObjects.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Index(nameof(Id), nameof(Email), nameof(Phone), IsUnique = true)]
public class User
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; }
    
    [Column("name")]
    [MaxLength(500)]
    public string Name { get; set; }
    
    [Column("email")]
    [MaxLength(320)]
    public string Email { get; }
    
    [Column("phone")]
    [MaxLength(30)]
    public string Phone { get; set; }
    
    [Column("password")]
    public string Password { get; private set; } = string.Empty;

    public HashSet<UserRoleEntity> Roles { get; private set; } = new HashSet<UserRoleEntity>();

    private protected User() { }

    public User(string name, string email, string phone)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Phone = phone;
    }

    public void AddRoles(UserRole role)
    {
        Roles.Add(new UserRoleEntity(this, role));
    }

    public void AddRoles(HashSet<UserRole> roles)
    {
        Roles.UnionWith(roles.Select(role => new UserRoleEntity(this, role)));
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
