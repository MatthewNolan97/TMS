using TMS_SharedLibrary.Models;

namespace TMSTeacher.Services
{
    public interface IToyAPIService
    {
        Task<IEnumerable<Toy>> GetAll();
        Task<IEnumerable<Toy>> GetFiltered(string[]? materials, string[]? categories);
        Task<Toy?> Get(int id);
        Task<bool> UpdateToy(int id, Toy t);
        Task<bool> Create(Toy toy);
        Task<bool> Delete(int id);
        Task<string?> UploadToyImageAsync(IFormFile file);
    }
}
