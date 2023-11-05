namespace Domain.Services.Authentication;

public interface IEncryptService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
