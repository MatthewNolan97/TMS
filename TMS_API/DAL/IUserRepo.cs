using TMS_SharedLibrary.Models;

namespace TMS_API.DAL
{
    public interface IUserRepo
    {
        User? FindById(int id);
        User? FindByOid(string oid);
        List<User> GetAll();
        User Create(User user);
    }
}
