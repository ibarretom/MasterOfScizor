using Domain.ValueObjects.Authentication;
using Domain.ValueObjects.DTO;

namespace Application.Services.Authentication;

internal interface ICompanyAuthentication
{
    public Task<TokenResponse> SignIn(SignInRequestDTO user);
}
