namespace Domain.ValueObjects.Authentication;

internal class TokenResponse
{
    public string Token { get; }
    public string RefreshToken { get; }
    public string UserName { get; }
    internal TokenResponse(string token, string refreshToken, string userName)
    {
        Token = token;
        RefreshToken = refreshToken;
        UserName = userName;
    }
}
