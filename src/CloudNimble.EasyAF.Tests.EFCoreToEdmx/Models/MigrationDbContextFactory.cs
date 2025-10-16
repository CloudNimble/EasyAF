using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models
{

    /// <summary>
    /// Base migration database context factory that implements the design-time factory interface.
    /// </summary>
    /// <remarks>
    /// This factory demonstrates direct implementation of <see cref="IDesignTimeDbContextFactory{TContext}"/>
    /// and serves as a base class for testing inheritance scenarios with design-time factories.
    /// </remarks>
    public class MigrationDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationDbContextFactory"/> class.
        /// </summary>
        public MigrationDbContextFactory()
        {
        }

        /// <summary>
        /// Creates a <see cref="TestDbContext"/> instance with the specified arguments.
        /// </summary>
        /// <param name="args">The arguments passed to the factory.</param>
        /// <returns>A configured <see cref="TestDbContext"/> instance.</returns>
        public virtual TestDbContext CreateDbContext(string[] args) => new(new DbContextOptions<TestDbContext>());

    }

}
