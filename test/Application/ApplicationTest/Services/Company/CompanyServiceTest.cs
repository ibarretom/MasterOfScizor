using Application.Services.Company;
using Domain.Entities.Barbers;
using Domain.Exceptions;
using Domain.Exceptions.Messages;
using Domain.ValueObjects.Addresses;
using DomainTest.ValueObjects.DTO;
using Infra.Repositories.Addresses;
using Infra.Repositories.CompanyRepository;
using Moq;

namespace ApplicationTest.Services.Company;

public class CompanyServiceTest
{
    [Fact]
    public async void ShouldBeAbleToCreateACompany()
    {
        var result = new Barber(Guid.Empty, string.Empty, string.Empty, string.Empty);
        var companyDTO = CreateCompanyDTOBuilder.Build();

        var addressLocalizationRepositoryMock = new Mock<IAddressLocalizationRepository>();
        addressLocalizationRepositoryMock.Setup(repository => repository.GetById(It.IsAny<Guid>()).Result)
            .Returns(new AddressLocalization(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));

        var companyRepositoryMock = new Mock<ICompanyRepository>();
        companyRepositoryMock.Setup(repository => repository.Create(It.IsAny<Barber>()))
            .Callback<Barber>((company) =>
            {
                result = company;
            })
            .Returns(Task.CompletedTask);

        var companyService = new CompanyService(companyRepositoryMock.Object, addressLocalizationRepositoryMock.Object);
        await companyService.Create(companyDTO);

        companyRepositoryMock.Verify(repository => repository.Create(It.IsAny<Barber>()), Times.Once);
        Assert.Equal(result.Name, companyDTO.Name);
        Assert.Equal(result.Identifier, companyDTO.Identifier);
        Assert.Equal(result.OwnerId, companyDTO.OwnerId);
        Assert.Equal(result.Branch.First().Identifier, companyDTO.Branch.Identifier);
        Assert.Equal(result.Branch.First().Address.AddressId, companyDTO.Branch.Address.AddressId);
        Assert.Equal(result.Branch.First().Address.Identifier.Number, companyDTO.Branch.Address.Identifier.Number);
        Assert.Equal(result.Branch.First().Address.Identifier.Complement, companyDTO.Branch.Address.Identifier.Complement);
        Assert.Equal(result.Branch.First().Phone, companyDTO.Branch.Phone);
        Assert.Equal(result.Branch.First().Email, companyDTO.Branch.Email);
    }

    [Fact]
    public async void ShouldNotBeAbleToCreateACompanyThatAlreadyExists()
    {
        var companyDTO = CreateCompanyDTOBuilder.Build();

        var addressLocalizationRepositoryMock = new Mock<IAddressLocalizationRepository>();
        addressLocalizationRepositoryMock.Setup(repository => repository.GetById(It.IsAny<Guid>()).Result)
            .Returns(new AddressLocalization(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));

        var companyRepositoryMock = new Mock<ICompanyRepository>();
        companyRepositoryMock.Setup(repository => repository.GetByCompanyIdentifier(It.IsAny<string>()).Result)
            .Returns(new Barber(Guid.Empty, string.Empty, string.Empty, string.Empty));

        var companyService = new CompanyService(companyRepositoryMock.Object, addressLocalizationRepositoryMock.Object);

        Func<Task> action = async () => await companyService.Create(companyDTO);
        var error = await Assert.ThrowsAsync<Exception>(action);

        Assert.Equal("Company already exists", error.Message);
    }

    [Fact]
    public async void ShouldNotBeAbelToCreateACompanyWithAnNotExistentAddress()
    {
        var companyDTO = CreateCompanyDTOBuilder.Build();

        var addressLocalizationRepositoryMock = new Mock<IAddressLocalizationRepository>();
        var companyRepositoryMock = new Mock<ICompanyRepository>();

        var companyService = new CompanyService(companyRepositoryMock.Object, addressLocalizationRepositoryMock.Object);

        Func<Task> action = async () => await companyService.Create(companyDTO);
        var error = await Assert.ThrowsAsync<AddressException>(action);

        Assert.Equal(AddressExceptionMessagesResource.ADDRESS_NOT_FOUND, error.Message);
    }
}
