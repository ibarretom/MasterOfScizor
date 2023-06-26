namespace Domain.Services.Authentication;

public interface IEncriptService
{
    string Hash(string password);
}
