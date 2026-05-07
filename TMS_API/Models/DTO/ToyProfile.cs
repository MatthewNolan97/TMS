using AutoMapper;
using TMS_SharedLibrary.Models;
namespace TMS_API.Models.DTO
{
    public class ToyProfile:Profile
    {
        public ToyProfile()
        {
            CreateMap<Toy, ToyDTO>().ReverseMap();
        }
    }
}
