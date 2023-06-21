using Domain.ValueObjects;

namespace Domain.Entities;

public class User
{
    public Guid Id { get; }
    public string Name { get; set; }
    public string Email { get; }
    public string Phone { get; set; }
    public string Password { get; set; }
    public UserRole Role { get; }
    public DateTime CreatedAt { get; }

    public User(string name, string email, string phone, string password, UserRole role)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        Phone = phone;
        Password = password;
        Role = role;
        CreatedAt = DateTime.Now;
    }

}
