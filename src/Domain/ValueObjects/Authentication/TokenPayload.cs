using Domain.ValueObjects.Enums;

namespace Domain.ValueObjects.Authentication;

internal class TokenPayload
{
    public Guid UserId { get; }
    public Guid BranchId { get; }
    public HashSet<UserRole> Roles { get; }
    public DateTime ExpiresIn { get; }

    public TokenPayload(Guid userId, Guid branchId, HashSet<UserRole> roles, DateTime expiresIn)
    {
        UserId = userId;
        BranchId = branchId;
        Roles = roles;
        ExpiresIn = expiresIn;
    }
}
