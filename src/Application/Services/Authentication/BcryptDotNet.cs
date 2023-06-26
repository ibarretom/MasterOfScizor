using Domain.Services.Authentication;

namespace Application.Services.Authentication;

internal class BcryptDotNet : IEncriptService
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
