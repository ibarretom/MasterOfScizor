using Application.Services.Authentication;
using AutoMapper;
using Domain.Entities;
using Domain.Services.Authentication;
using Domain.Services.Mappers;
using Domain.ValueObjects;
using DomainTest.ValueObjects.DTO;
using Infra.Repositories.UserRepository;
using Moq;

namespace ApplicationTest.Services.Authentication;

public class AuthenticationServiceTest
{
    [Fact]
    public void ShouldBeAbleToSignUp()
    {
        var timeInTestBegining = DateTime.Now;
        var user = UserDTOBuilder.Build();
        var hashedPass = "Hashed";

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        }).CreateMapper();

        var encriptService = new Mock<IEncriptService>();
        encriptService.Setup(service => service.Hash(It.IsAny<string>())).Returns(hashedPass);

        var userSaved = new User(string.Empty, string.Empty, string.Empty);

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(repository => repository.Create(It.IsAny<User>()))
            .Callback((User user) =>
            {
                userSaved = user;
            });

        var authenticationService = new AuthenticationService(userRepository.Object, encriptService.Object, mapper);
        authenticationService.SignUp(user);

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
}