using TMS_SharedLibrary.Models;

namespace TMSTeacher.Services {
    public interface ITeacherAPIService {
        Task<List<Teacher>> GetAll();
        Task<Teacher?> Get(int id);
        Task<Teacher> Create(Teacher teacher);
    }
}
