using Application.Services.Authentication;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Authentication;
using Domain.Services.Mappers;
using Domain.ValueObjects.Authentication;
using Domain.ValueObjects.Enums;
using DomainTest.Entities;
using DomainTest.ValueObjects.DTO;
using Infra.Repositories.UserRepository;
using Moq;

namespace ApplicationTest.Services.Authentication;

public class AuthenticationServiceTest
{
    [Fact]
    public async Task ShouldBeAbleToSignUp()
    {
        var timeInTestBegining = DateTime.Now;
        var user = UserDTOBuilder.Build();
        var hashedPass = "Hashed";

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        }).CreateMapper();

        var encriptService = new Mock<IEncryptService>();
        encriptService.Setup(service => service.Hash(It.IsAny<string>())).Returns(hashedPass);

        var userSaved = new User(string.Empty, string.Empty, string.Empty);

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(repository => repository.Create(It.IsAny<User>()))
            .Callback((User user) =>
            {
                userSaved = user;
            });

        var tokenService = new Mock<ITokenService>();

        var authenticationService = new AuthenticationService(userRepository.Object, encriptService.Object, mapper, tokenService.Object);
        await authenticationService.SignUp(user);

        encriptService.Verify(service => service.Hash(It.IsAny<string>()), Times.Once);
        userRepository.Verify(repository => repository.Create(It.IsAny<User>()), Times.Once);

        Assert.True(!userSaved.Id.Equals(Guid.Empty));
        Assert.True(userSaved.Name.Equals(user.Name));
        Assert.True(userSaved.Email.Equals(user.Email));
        Assert.True(userSaved.Phone.Equals(user.Phone));
        Assert.True(userSaved.Password.Equals(hashedPass));
        Assert.True(userSaved.Roles.Count.Equals(1));
        Assert.True(userSaved.Roles.First().Equals(UserRole.Customer));
        Assert.True(DateTime.Compare(userSaved.CreatedAt, timeInTestBegining) > 0);
    }

    [Fact]
    public async Task ShouldBeAbleToSignInAUser()
    {
        var userDTO = SignInRequestDTOBuilder.Build("email");

        var encryptService = new Mock<IEncryptService>();
        encryptService.Setup(service => service.Hash(It.IsAny<string>())).Returns("Hashed");
        
        var user = UserBuilder.Build();
        user.SetPassword("notHashed", encryptService.Object);

        encryptService.Setup(service => service.Verify(User.GetPasswordToBeHashed(user, userDTO.Password), "Hashed")).Returns(true);
        
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(repository => repository.FindByCredential(userDTO.Credential).Result).Returns(user);

        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(service => service.Sign(It.IsAny<TokenPayload>())).Returns("token");

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        }).CreateMapper();

        var authenticationService = new AuthenticationService(userRepository.Object, encryptService.Object, mapper, tokenService.Object);

        var token = await authenticationService.SignIn(userDTO);

        userRepository.Verify(repository => repository.FindByCredential(userDTO.Credential), Times.Once);

        Assert.Equal(token.UserName, user.Name);
        Assert.Equal("token", token.Token);
        Assert.Equal("token", token.RefreshToken);
    }

    [Fact]
    public async Task ShouldThrowsWhenUserIsNotFound()
    {
        var userDTO = SignInRequestDTOBuilder.Build("phone");

        var encryptService = new Mock<IEncryptService>();
        encryptService.Setup(repository => repository.Hash(It.IsAny<string>())).Returns("Hashed");

        var userRepository = new Mock<IUserRepository>();
        
        var tokenService = new Mock<ITokenService>();

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        }).CreateMapper();

        var authenticationService = new AuthenticationService(userRepository.Object, encryptService.Object, mapper, tokenService.Object);

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() => authenticationService.SignIn(userDTO));

        Assert.Equal(AuthenticationExceptionMessagesResource.CREDENTIALS_DOES_NOT_MATCH, exception.Message);
    }

    [Fact]
    public async Task ShouldThrowsWhenPasswordDoesNotMatches()
    {
        var userDTO = SignInRequestDTOBuilder.Build("phone");

        var encryptService = new Mock<IEncryptService>();
        encryptService.Setup(repository => repository.Hash(It.IsAny<string>())).Returns("Hashed");
        encryptService.Setup(repository => repository.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var user = UserBuilder.Build();
        user.SetPassword("notHashed", encryptService.Object);

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(repository => repository.FindByCredential(userDTO.Credential).Result).Returns(user);

        var tokenService = new Mock<ITokenService>();

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        }).CreateMapper();

        var authenticationService = new AuthenticationService(userRepository.Object, encryptService.Object, mapper, tokenService.Object);

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() => authenticationService.SignIn(userDTO));

        Assert.Equal(AuthenticationExceptionMessagesResource.CREDENTIALS_DOES_NOT_MATCH, exception.Message);
    }
}