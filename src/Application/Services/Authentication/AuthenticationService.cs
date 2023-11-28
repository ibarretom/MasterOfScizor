using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Authentication;
using Domain.ValueObjects.Authentication;
using Domain.ValueObjects.DTO;
using Domain.ValueObjects.Enums;
using Infra.Repositories.UserRepository;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ApplicationTest")]
namespace Application.Services.Authentication;

internal class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptService _encryptService;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;

    public AuthenticationService(IUserRepository userRepository, IEncryptService encryptService, IMapper mapper, ITokenService tokenService)
    {
        _encryptService = encryptService;
        _userRepository = userRepository;
        _mapper = mapper;
        _tokenService = tokenService;
    }

    public async Task SignUp(UserRequestDTO user)
    {   
        var created_user = _mapper.Map<User>(user);
        created_user.AddRoles(UserRole.Customer);
        created_user.SetPassword(user.Password, _encryptService);

        await _userRepository.Create(created_user);
    }

    public async Task<TokenResponse> SignIn(SignInRequestDTO user)
    {
        var existentUser = await _userRepository.FindByCredential(user.Credential) 
            ?? throw new AuthenticationException(AuthenticationExceptionMessagesResource.CREDENTIALS_DOES_NOT_MATCH);

        if (!_encryptService.Verify(User.GetPasswordToBeHashed(existentUser, user.Password), existentUser.Password))
            throw new AuthenticationException(AuthenticationExceptionMessagesResource.CREDENTIALS_DOES_NOT_MATCH);

        var roles = existentUser.Roles.Select(role => role.Role).ToHashSet();
        var token = _tokenService.Sign(new TokenPayload(existentUser.Id, Guid.Empty, roles, DateTime.UtcNow.AddHours(2)));
        var refreshToken = _tokenService.Sign(new TokenPayload(existentUser.Id, Guid.Empty, roles, DateTime.UtcNow.AddDays(30)));

        return new TokenResponse(token, refreshToken, existentUser.Name);
    }
}
