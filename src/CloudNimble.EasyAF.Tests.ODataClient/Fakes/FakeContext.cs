using Microsoft.EntityFrameworkCore;

namespace CloudNimble.EasyAF.Tests.OData.Fakes
{

    /// <summary>
    /// A fake <see cref="DbContext"/> for unit testing.
    /// </summary>
    public class FakeContext : DbContext
    {

        /// <summary>
        /// Fake POCO entity.
        /// </summary>
        public DbSet<FakeEntity> Entities { get; set; }

        /// <summary>
        /// Constructor overload to send <see cref="DbContextOptions{TContext}"/> to the base class.
        /// </summary>
        /// <param name="options"></param>
        public FakeContext(DbContextOptions<FakeContext> options) : base(options)
        {
        }

        /// <summary>
        /// Context configuration.
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(nameof(FakeContext));
        }

        /// <summary>
        /// Context seeding for unit tests.
        /// </summary>
        /// <param name="context"></param>
        public static void Seed(FakeContext context)
        {
            context.Entities.AddRange(new[] {
                new FakeEntity { Id = 1, Name = "Entity 1", Description = "A fake entity." },
                new FakeEntity { Id = 2, Name = "Entity 2", Description = "Another fake entity." },
                new FakeEntity { Id = 3, Name = "Entity 31", Description = "Still another fake entity." },
            });
            context.SaveChanges();
        }

    }

}
