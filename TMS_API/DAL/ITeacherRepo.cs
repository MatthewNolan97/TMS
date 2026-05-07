using TMS_SharedLibrary.Models;

namespace TMS_API.DAL
{
    public interface ITeacherRepo
    {
        List<Teacher> GetAll();
        Teacher? FindById(int id);
        Teacher Create(Teacher teacher);
    }
}
