using Domain.ValueObjects.Authentication;

namespace Application.Services.Authentication;

internal interface ITokenService
{
    public string Sign(TokenPayload tokenPayload);
}
