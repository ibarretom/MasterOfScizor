using AutoMapper;
using Domain.Services.Mappers;

namespace DomainTest.Services.Mapper;

public class AutoMapperFixture : IDisposable
{
    public IMapper Mapper { get; }
    public AutoMapperFixture()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        });
        Mapper = config.CreateMapper();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

