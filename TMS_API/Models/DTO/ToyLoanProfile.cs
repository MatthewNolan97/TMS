using AutoMapper;
using TMS_SharedLibrary.Models;

namespace TMS_API.Models.DTO
{
    public class ToyLoanProfile:Profile
    {
        public ToyLoanProfile()
        {
            CreateMap<ToyLoan, ToyLoanDTO>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Toy.Name))
                .ReverseMap();
        }
    }
}
