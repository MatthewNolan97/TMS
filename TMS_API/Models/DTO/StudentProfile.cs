using AutoMapper;
using TMS_SharedLibrary.Models;

namespace TMS_API.Models.DTO
{
    public class StudentProfile: Profile
    {
        public StudentProfile() {
            CreateMap<Student, StudentDTO>()
                 .ForMember(dest => dest.OID, opt => opt.MapFrom(src => src.User.Oid))
                 .ForMember(dest => dest.BorrowedToysHistory, opt => opt.MapFrom(src =>
                     src.ToyLoans.Select(tl => tl.Toy.Name).ToList()))
                 .ForMember(dest => dest.CurrentBorrowedToys, opt => opt.MapFrom(src => 
                 src.ToyLoans
                 .Where(tl=> tl.ReturnDate == null)
                 .Select(tl => tl.Toy.Name).ToList()));
                  
            CreateMap<StudentDTO, Student>()
                .ForMember(dest => dest.ToyLoans, opt => opt.Ignore())
                .ForMember(dest=> dest.User, opt => opt.Ignore());
        }
    }
}
