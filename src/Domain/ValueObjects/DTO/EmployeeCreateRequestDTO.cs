using Domain.ValueObjects.Enums;

namespace Domain.ValueObjects.DTO;

internal class EmployeeCreateRequestDTO
{
    public Guid BranchId { get; }
    public string Name { get; }
    public string Email { get; }
    public string Phone { get; }
    public string Password { get; }
    public string Document { get; }
    public bool Active { get; }
    public string Avatar { get; set; }
    public HashSet<UserRole> UserRoles { get; }

    public EmployeeCreateRequestDTO(Guid branchId, string name, string email, string phone, string password, string avatar, bool active, string document, HashSet<UserRole> userRoles)
    {
        BranchId = branchId;
        Name = name;
        Email = email;
        Phone = phone;
        Avatar = avatar;
        Active = active;
        Document = document;
        Password = password;
        UserRoles = userRoles;
    }
}
