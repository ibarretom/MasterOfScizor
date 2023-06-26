using AutoMapper;
using Domain.Entities;
using Domain.Services.Mappers;
using DomainTest.ValueObjects.DTO;

namespace DomainTest.Services.Mapper;


[CollectionDefinition("AutoMapperFixture")]
public class AutoMapperTest : ICollectionFixture<AutoMapperFixture> { }

[Collection("AutoMapperFixture")]
public class AutoMapperProfileTest
{
    private readonly IMapper _mapper;
    public AutoMapperProfileTest(AutoMapperFixture fixture) => _mapper = fixture.Mapper;

    [Fact]
    public void TestConfiguration()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(AutoMapperProfile).Assembly);
        });
        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void ShouldBeAbleToMapUserDtoToUser()
    {
        var userDTO = UserDTOBuilder.Build();

        var user = _mapper.Map<User>(userDTO);

        Assert.True(user.Name.Equals(userDTO.Name));
        Assert.True(user.Email.Equals(userDTO.Email));
        Assert.True(user.Phone.Equals(userDTO.Phone));
    }
}
