using TMS_SharedLibrary.Models;

namespace TMSStudent.Services
{
    public interface IUserAPIService
    {
        Task<User> GetUserByOidAsync(string oid);
        Task CreateUserAsync(User user);
    }
}
