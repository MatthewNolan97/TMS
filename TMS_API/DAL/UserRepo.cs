using Microsoft.EntityFrameworkCore;
using TMS_SharedLibrary.Models;

namespace TMS_API.DAL
{
    public class UserRepo : IUserRepo
    {
        private readonly K40TmsDdContext _context;

        public UserRepo(K40TmsDdContext context)
        {
            _context = context;
        }

        public User? FindById(int id)
        {
            return _context.Users
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .FirstOrDefault(u => u.UserId == id);
        }

        public List<User> GetAll()
        {
            return _context.Users
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .OrderBy(u => u.UserId)
                .ToList();
        }

        public User? FindByOid(string oid)
        {
            return _context.Users
                .Include(u => u.Student)
                .Include(u => u.Teacher)
                .FirstOrDefault(u => u.Oid == oid);
        }

        public User Create(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }
    }
}
