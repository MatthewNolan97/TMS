using TMS_SharedLibrary.Models;

namespace TMSStudent.Services
{
    public interface IToyAPIService
    {
        Task<IEnumerable<Toy>> GetAll();
        Task<Toy?> Get(int id);
        Task<bool> UpdateToy(int id, Toy t);
        Task<bool> Create(Toy toy);
        Task<bool> Delete(int id);
        Task<IEnumerable<Toy>> GetFiltered(string[]? materials, string[]? categories);
        Task<bool> ReturnToy(int studentid, int toyid);
        Task<bool> BorrowToy(int studentid, int toyid);
    }
}

