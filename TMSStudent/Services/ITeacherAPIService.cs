using TMS_SharedLibrary.Models;

namespace TMSStudent.Services {
    public interface ITeacherAPIService {
        Task<List<Teacher>> GetAll();
        Task<Teacher?> Get(int id);
        Task<Teacher> Create(Teacher teacher);
    }
}
