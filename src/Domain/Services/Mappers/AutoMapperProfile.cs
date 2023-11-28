using AutoMapper;
using Domain.Entities;
using Domain.ValueObjects.DTO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ApplicationTest")]
[assembly: InternalsVisibleTo("DomainTest")]
namespace Domain.Services.Mappers;

internal class AutoMapperProfile : Profile
{
        public AutoMapperProfile()
        {
            CreateMap<UserRequestDTO, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore());
        }
}
