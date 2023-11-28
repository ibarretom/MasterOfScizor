using Domain.Entities;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Authentication;
using Domain.ValueObjects.Authentication;
using Domain.ValueObjects.DTO;
using Infra.Repositories.CompanyRepository;
using Infra.Repositories.UserRepository;

namespace Application.Services.Authentication;

internal class CompanyAuthentication : ICompanyAuthentication
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ITokenService _tokenService;
    private readonly IEncryptService _encryptService;

    public CompanyAuthentication(ICompanyRepository companyRepository, ITokenService tokenService, IEncryptService encryptService)
    {
        _companyRepository = companyRepository;
        _tokenService = tokenService;
        _encryptService = encryptService;
    }

    public async Task<TokenResponse> SignIn(SignInRequestDTO user)
    {
        var employee = await _companyRepository.FindEmployee(user.Credential) ?? throw new AuthenticationException(AuthenticationExceptionMessagesResource.CREDENTIALS_DOES_NOT_MATCH);

        if (!_encryptService.Verify(employee.Password, User.GetPasswordToBeHashed(employee, employee.Password)))
            throw new AuthenticationException(AuthenticationExceptionMessagesResource.CREDENTIALS_DOES_NOT_MATCH);

        var roles = employee.Roles.Select(role => role.Role).ToHashSet();
        var token = _tokenService.Sign(new TokenPayload(employee.Id, employee.Branch.Id, roles , DateTime.UtcNow.AddHours(2)));
        var refreshToken = _tokenService.Sign(new TokenPayload(employee.Id, employee.Branch.Id, roles, DateTime.UtcNow.AddDays(30)));

        return new TokenResponse(token, refreshToken, employee.Name);
    }
}
