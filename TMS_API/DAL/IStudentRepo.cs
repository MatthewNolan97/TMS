using TMS_SharedLibrary.Models;

namespace TMS_API.DAL
{
    public interface IStudentRepo
    {
        List<Student> GetAll();
        Student? FindById(int id);
        void BookToy(int studentId, int toyId);
        void ReturnToy(int studentId, int toyId);
        bool checkToyIsBorrowedByStudent(int studentId, int toyId);
        List<ToyLoan> GetBorrowHistory(int studentId);
        int? FindByOid(string oid);
    }
}
