using Microsoft.EntityFrameworkCore;
using TMS_SharedLibrary.Models;

namespace TMS_API.DAL
{
    public class StudentRepo : IStudentRepo
    {
        private readonly K40TmsDdContext _context;
        public StudentRepo(K40TmsDdContext context)
        {
            _context = context;
        }

        public void BookToy(int studentId, int toyId)
        {
            var toyLoan = new ToyLoan
            {
                StudentId = studentId,
                ToyId = toyId,
                BorrowDate = DateOnly.FromDateTime(DateTime.Now),
                DueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3 * 7)),
                ReturnDate = null

            };
            _context.ToyLoans.Add(toyLoan);
            var toy = _context.Toys.FirstOrDefault(t => t.ToyId == toyId);
            if (toy != null)
            {
                toy.IsAvailable = false;
                _context.Toys.Update(toy);
            }
            _context.SaveChanges();
        }

        public void ReturnToy(int studentId, int toyId)
        {
            var toyLoan = _context.ToyLoans
                .Where(tl => tl.StudentId == studentId && tl.ToyId == toyId && tl.ReturnDate == null)
                .FirstOrDefault();
            if (toyLoan != null)
            {
                var toy = _context.Toys.FirstOrDefault(t => t.ToyId == toyId);
                if (toy != null)
                {
                    toy.IsAvailable = true;
                    _context.Toys.Update(toy);
                    toyLoan.ReturnDate = DateOnly.FromDateTime(DateTime.Now);
                    _context.ToyLoans.Update(toyLoan);
                    _context.SaveChanges();
                }
            }
        }
        public bool checkToyIsBorrowedByStudent(int studentId, int toyId)
        {
            var toyLoan = _context.ToyLoans
                .Where(tl => tl.StudentId == studentId && tl.ToyId == toyId && tl.ReturnDate == null)
                .FirstOrDefault();
            if(toyLoan != null)
            {
                return true;
            }
            return false;
        }

        public Student? FindById(int id)
        {
            return _context.Students
                .Include(s => s.User)
                .Include(s => s.ToyLoans)
                .ThenInclude(tl => tl.Toy)
                .FirstOrDefault(s => s.StudentId == id);
        }

        public List<Student> GetAll()
        {
            return _context.Students
                .Include(s => s.User)
                .Include(s => s.ToyLoans)
                .ThenInclude(tl => tl.Toy)
                .OrderBy(s => s.User.UserId)
                .ToList();
        }
        public int? FindByOid(string oid)
        {
            return _context.Students
               .Where(s => s.User.Oid == oid)
               .Select(s => s.StudentId)
               .FirstOrDefault();
        }

        public List<ToyLoan> GetBorrowHistory(int studentId)
        {
            return _context.ToyLoans
                .Include(tl => tl.Toy)
                .Where(tl => tl.StudentId == studentId)
                .OrderByDescending(tl => tl.BorrowDate)
                .ToList();
        }

    }
}
