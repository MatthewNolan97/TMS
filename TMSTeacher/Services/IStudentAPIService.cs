using TMS_SharedLibrary.Models;

namespace TMSTeacher.Services
{
    public interface IStudentAPIService
    {
        Task<List<Student>> GetAll();
        Task<Student?> Get(int id);
        Task Create(Student student);
        Task Update(Student student);
        Task Delete(int id);
        Task<List<User>> GetUsers();
        Task<List<ToyLoan>> GetBorrowHistory(int studentId);
    }
}
