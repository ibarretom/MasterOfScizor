using Domain.Entities;
using Domain.ValueObjects.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.ValueObjects.Barbers;

public class UserRoleEntity
{
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; private set; }

    public User User { get; init;  }

    [Column("role")]
    public UserRole Role { get; init; }

    public UserRoleEntity(User user, UserRole role)
    {
        User = user;
        Role = role;
    }

    private UserRoleEntity() { }
}
