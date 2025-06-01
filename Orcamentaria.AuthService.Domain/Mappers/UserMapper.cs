using Orcamentaria.AuthService.Domain.DTOs.User;
using Orcamentaria.AuthService.Domain.Models;
using AutoMapper;

namespace Orcamentaria.AuthService.Domain.Mappers
{
    public class UserMapper : Profile
    {
        public UserMapper() 
        {
            CreateMap<User, UserInsertDTO>()
                .ForMember(s => s.Name, opt => opt.MapFrom(d => d.Name))
                .ForMember(s => s.Email, opt => opt.MapFrom(d => d.Email))
                .ForMember(s => s.Password, opt => opt.MapFrom(d => d.Password))
                .ReverseMap();

            CreateMap<User, UserUpdateDTO>()
                .ForMember(s => s.Name, opt => opt.MapFrom(d => d.Name))
                .ForMember(s => s.Active, opt => opt.MapFrom(d => d.Active))
                .ReverseMap();

            CreateMap<UserResponseDTO, User>()
                .ForMember(s => s.Id, opt => opt.MapFrom(d => d.Id))
                .ForMember(s => s.Name, opt => opt.MapFrom(d => d.Name))
                .ForMember(s => s.Email, opt => opt.MapFrom(d => d.Email))
                .ForMember(s => s.CompanyId, opt => opt.MapFrom(d => d.CompanyId))
                .ReverseMap();
        }
    }
}
