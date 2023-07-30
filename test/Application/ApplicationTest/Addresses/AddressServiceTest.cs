using Application.Services.Addresses;
using Domain.ValueObjects.Addresses;
using DomainTest.ValueObjects.DTO;
using Infra.Repositories.Addresses;
using Moq;

namespace ApplicationTest.Addresses;

public class AddressServiceTest
{
    [Fact]
    public async void ShouldBeAbleToCreateANewAddress()
    {
        var addressDTO = AddressLocalizationCreateDTOBuilder.Build();

        var addressRepository = new Mock<IAddressLocalizationRepository>();
        addressRepository.Setup(repository => repository.Add(It.IsAny<AddressLocalization>()).Result)
            .Returns(new AddressLocalization(addressDTO.Country, addressDTO.State, addressDTO.City, addressDTO.Neighborhood, addressDTO.Street, addressDTO.ZipCode));
        var addressService = new AddressService(addressRepository.Object);

        var address = await addressService.AddOrRetrieve(addressDTO);

        Assert.True(!address.Id.Equals(Guid.Empty));
        Assert.True(address.City.Equals(addressDTO.City));
        Assert.True(address.Country.Equals(addressDTO.Country));
        Assert.True(address.Neighborhood.Equals(addressDTO.Neighborhood));
        Assert.True(address.State.Equals(addressDTO.State));
        Assert.True(address.Street.Equals(addressDTO.Street));
        Assert.True(address.ZipCode.Equals(addressDTO.ZipCode));
    }

    [Fact]
    public async void ShouldBeAbleToRetrieveAnExistingAddress()
    {
        var addressDTO = AddressLocalizationCreateDTOBuilder.Build();

        var addressRepository = new Mock<IAddressLocalizationRepository>();
        addressRepository.Setup(repository => repository.GetByZipCode(addressDTO.ZipCode).Result)
            .Returns(new AddressLocalization(addressDTO.Country, addressDTO.State, addressDTO.City, addressDTO.Neighborhood, addressDTO.Street, addressDTO.ZipCode));
        
        var addressService = new AddressService(addressRepository.Object);
        
        var address = await addressService.AddOrRetrieve(addressDTO);

        Assert.True(!address.Id.Equals(Guid.Empty));
        Assert.True(address.City.Equals(addressDTO.City));
        Assert.True(address.Country.Equals(addressDTO.Country));
        Assert.True(address.Neighborhood.Equals(addressDTO.Neighborhood));
        Assert.True(address.State.Equals(addressDTO.State));
        Assert.True(address.Street.Equals(addressDTO.Street));
        Assert.True(address.ZipCode.Equals(addressDTO.ZipCode));
    }
}
