using Domain.ValueObjects.Authentication;
using Domain.ValueObjects.DTO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("ApplicationTest")]
namespace Application.Services.Authentication;

internal interface IAuthenticationService
{
    Task SignUp(UserRequestDTO user);
    Task<TokenResponse> SignIn(SignInRequestDTO user);
}
