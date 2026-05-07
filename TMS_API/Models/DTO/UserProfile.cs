using AutoMapper;
using TMS_SharedLibrary.Models;

namespace TMS_API.Models.DTO
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();
        }
    }
}
