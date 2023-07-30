namespace Domain.ValueObjects.DTO;

public class SignInRequestDTO
{
    public string Credential { get; set; }
    public string Password { get; set; }

    public SignInRequestDTO(string credential, string password)
    {
        Credential = credential;
        Password = password;
    }
}
