using Microsoft.EntityFrameworkCore;
using TMS_SharedLibrary.Models;

namespace TMS_API.DAL
{
    public class TeacherRepo : ITeacherRepo
    {
        private readonly K40TmsDdContext _context;
        public TeacherRepo(K40TmsDdContext context)
        {
            _context = context;
        }
        public Teacher? FindById(int id)
        {
          return _context.Teachers.FirstOrDefault(t => t.TeacherId == id);
        }

        public List<Teacher> GetAll()
        {
            return _context.Teachers
                .Include(t => t.User)
                .ToList();
        }

        public Teacher Create(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            _context.SaveChanges();
            return teacher;
        }
    }
}
