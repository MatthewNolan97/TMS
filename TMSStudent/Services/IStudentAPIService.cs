using TMS_SharedLibrary.Models;

namespace TMSStudent.Services
{
    public interface IStudentAPIService
    {
        Task<List<Student>> GetAll();
        Task<Student?> Get(int id);
        Task<int?> GetStudentID(string oid);
        Task Create(Student student);
        Task Update(Student student);
        Task Delete(int id);
        Task<List<User>> GetUsers();
        Task<List<ToyLoan>> GetBorrowHistory(int studentId);
        //Task<List<ToyLoan>> GetActiveLoans(int studentId);
        Task<List<ToyLoan>> GetCurrentLoans(int studentId);
    }
}
