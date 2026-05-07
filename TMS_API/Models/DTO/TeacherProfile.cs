using AutoMapper;
using TMS_SharedLibrary.Models;
namespace TMS_API.Models.DTO
{
    public class TeacherProfile : Profile
    {
        public TeacherProfile()
        {
            CreateMap<Teacher, TeacherDTO>()
                .ForMember(dest => dest.OID,
                opt => opt.MapFrom(src => src.User.Oid))
                .ReverseMap();
        }
    }
}
