using Domain.ValueObjects.DTO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("ApplicationTest")]
namespace Application.Services.Authentication;

internal interface IAuthenticationService
{
    void SignUp(UserRequestDTO user);
}
