using TMS_SharedLibrary.Models;

namespace TMSTeacher.Services
{
    public interface IUserAPIService
    {
        Task<User> GetUserByOidAsync(string oid);
        Task<User> CreateUserAsync(User user);
    }
}
