using Microsoft.EntityFrameworkCore;
using TMS_SharedLibrary.Models;

namespace TMS_API.DAL
{
    public class ToyRepo : IToyRepo
    {
        private readonly K40TmsDdContext _context;
        public ToyRepo(K40TmsDdContext context)
        {
            _context = context;
        }
        public void Add(Toy toy)
        {
            _context.Toys.Add(toy);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
           var toy = _context.Toys.Find(id);
              if (toy != null)
              {

                toy.IsActive = false;
                _context.SaveChanges();
            }
        }

        public Toy? FindById(int id)
        {
            return _context.Toys.FirstOrDefault(t => t.ToyId == id && t.IsActive);
        }

        public List<Toy> GetAll()
        {
            return _context.Toys
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .Include(t => t.ToyLoans)
                .ToList();
        }

        public List<Toy> GetFiltered(IEnumerable<string>? materials, IEnumerable<string>? categories)
        {
            var query = _context.Toys.Where(t => t.IsActive).AsQueryable();

            if (materials != null && materials.Any())
            {
                query = query.Where(t => materials.Contains(t.Material));
            }

            if (categories != null && categories.Any())
            {
                query = query.Where(t => categories.Contains(t.Category));
            }

            return query.OrderBy(t => t.Name).ToList();
        }

        public void Update(int id, Toy toy)
        {
           var old_toy = _context.Toys.Find(id);
            if (old_toy != null)
            {
                old_toy.Name = toy.Name;
                old_toy.Description = toy.Description;
                old_toy.Category = toy.Category;
                old_toy.Material = toy.Material;
                old_toy.LocationCode = toy.LocationCode;
                old_toy.ImagePath = toy.ImagePath;
                old_toy.IsAvailable = toy.IsAvailable;
                old_toy.ManagedBy = toy.ManagedBy;
                old_toy.AdditionalInformation = toy.AdditionalInformation;
                _context.SaveChanges();
            }
        }
    }
}
