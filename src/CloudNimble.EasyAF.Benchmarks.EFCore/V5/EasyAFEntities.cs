using EasyAFModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudNimble.EasyAF.Benchmarks.V5
{

    /// <summary>
    /// A code-first EF Core context over the EasyAF 5.0 generated entities.
    /// </summary>
    public class EasyAFEntities : DbContext
    {

        #region Properties

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductStatusType> ProductStatusTypes { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the context with the given options, such as an EF Core in-memory database.
        /// </summary>
        /// <param name="options">The context options.</param>
        public EasyAFEntities(DbContextOptions<EasyAFEntities> options) : base(options)
        {
        }

        #endregion

        #region Protected Methods

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().IgnoreTrackingFields();
            modelBuilder.Entity<ProductStatusType>().IgnoreTrackingFields();
            modelBuilder.Entity<Product>()
                .HasOne(c => c.StatusType)
                .WithMany(c => c.Products)
                .HasForeignKey(c => c.StatusTypeId);
        }

        #endregion

    }

}
