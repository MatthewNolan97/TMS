using Microsoft.EntityFrameworkCore;
using TMS_API.DAL;
using TMS_SharedLibrary.Models;
using Xunit;

namespace TMS_TEST.TMS_API.ServiceTests
{
    public class ToyRepoDeleteTests
    {
        private static K40TmsDdContext CreateInMemoryDb(string dbName)
        {
            var options = new DbContextOptionsBuilder<K40TmsDdContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new K40TmsDdContext(options);
        }

        [Fact]
        public void Delete_SetsIsActiveFalse_WhenToyExists()
        {
            var dbName = $"ToyRepoDeleteTests_{System.Guid.NewGuid():N}";
            using var context = CreateInMemoryDb(dbName);

            context.Toys.Add(new Toy { ToyId = 1, Name = "Ball", IsActive = true, IsAvailable = true });
            context.SaveChanges();

            var repo = new ToyRepo(context);
            repo.Delete(1);

            var updated = context.Toys.Single(t => t.ToyId == 1);
            Assert.False(updated.IsActive);
        }

        [Fact]
        public void Delete_DoesNothing_WhenToyMissing()
        {
            var dbName = $"ToyRepoDeleteTests_Missing_{System.Guid.NewGuid():N}";
            using var context = CreateInMemoryDb(dbName);

            var repo = new ToyRepo(context);
            repo.Delete(999);

            Assert.Empty(context.Toys);
        }
    }
}
