using Application.Services.Authentication;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.Services.Authentication;
using Domain.ValueObjects.Authentication;
using DomainTest.Entities;
using DomainTest.ValueObjects.DTO;
using Infra.Repositories.CompanyRepository;
using Moq;

namespace ApplicationTest.Services.Authentication
{
    public class CompanyAuthenticationService
    {
        [Fact]
        public async Task ShouldBeAbleToAuthenticateAEmployee()
        {
            var signInRequest = SignInRequestDTOBuilder.Build("email");

            var encryptService = new Mock<IEncryptService>();
            encryptService.Setup(service => service.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            var employee = EmployeeBuilder.Build();

            var companyRepository = new Mock<ICompanyRepository>();
            companyRepository.Setup(repository => repository.FindEmployee(signInRequest.Credential))
                .ReturnsAsync(employee);

            var tokenService = new Mock<ITokenService>();
            tokenService.Setup(service => service.Sign(It.IsAny<TokenPayload>())).Returns("token");

            var authenticationService = new CompanyAuthentication(companyRepository.Object, tokenService.Object, encryptService.Object);

            var tokenResponse = await authenticationService.SignIn(signInRequest);

            companyRepository.Verify(repository => repository.FindEmployee(It.IsAny<string>()), Times.Once);

            Assert.Equal("token", tokenResponse.Token);
            Assert.Equal("token", tokenResponse.RefreshToken);
            Assert.Equal(employee.Name, tokenResponse.UserName);
        }

        [Fact]
        public async Task ShouldThrowsWhenUserIsNotFound()
        {
            var signInRequest = SignInRequestDTOBuilder.Build("email");

            var encryptService = new Mock<IEncryptService>();

            var companyRepository = new Mock<ICompanyRepository>();

            var tokenService = new Mock<ITokenService>();

            var authenticationService = new CompanyAuthentication(companyRepository.Object, tokenService.Object, encryptService.Object);

            var exception = await Assert.ThrowsAsync<AuthenticationException>(() => authenticationService.SignIn(signInRequest));

            Assert.Equal(AuthenticationExceptionMessagesResource.CREDENTIALS_DOES_NOT_MATCH, exception.Message);
        }

        [Fact]
        public async Task ShouldThrowsWhenPasswordDoesNotMatches()
        {
            var signInRequest = SignInRequestDTOBuilder.Build("email");

            var encryptService = new Mock<IEncryptService>();
            encryptService.Setup(service => service.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var employee = EmployeeBuilder.Build();

            var companyRepository = new Mock<ICompanyRepository>();
            companyRepository.Setup(repository => repository.FindEmployee(signInRequest.Credential))
                .ReturnsAsync(employee);

            var tokenService = new Mock<ITokenService>();
            tokenService.Setup(service => service.Sign(It.IsAny<TokenPayload>())).Returns("token");

            var authenticationService = new CompanyAuthentication(companyRepository.Object, tokenService.Object, encryptService.Object);

            var exception = await Assert.ThrowsAsync<AuthenticationException>(() => authenticationService.SignIn(signInRequest));

            Assert.Equal(AuthenticationExceptionMessagesResource.CREDENTIALS_DOES_NOT_MATCH, exception.Message);
        }
    }
}
