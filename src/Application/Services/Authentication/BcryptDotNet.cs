using Domain.Services.Authentication;

namespace Application.Services.Authentication;

internal class BcryptDotNet : IEncryptService
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
