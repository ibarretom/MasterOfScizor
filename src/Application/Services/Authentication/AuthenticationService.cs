using AutoMapper;
using Domain.Entities;
using Domain.Services.Authentication;
using Domain.ValueObjects.DTO;
using Domain.ValueObjects.Enums;
using Infra.Repositories.UserRepository;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ApplicationTest")]
namespace Application.Services.Authentication;

internal class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IEncriptService _encryptService;
    private readonly IMapper _mapper;

    public AuthenticationService(IUserRepository userRepository, IEncriptService encryptService, IMapper mapper)
    {
        _encryptService = encryptService;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async void SignUp(UserRequestDTO user)
    {   
        var created_user = _mapper.Map<User>(user);
        created_user.AddRoles(UserRole.Customer);
        created_user.SetPassword(user.Password, _encryptService);

        await _userRepository.Create(created_user);
    }
}
