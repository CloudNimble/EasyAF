using Microsoft.EntityFrameworkCore;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models
{

    /// <summary>
    /// Custom database context factory that derives from a base factory for testing inheritance scenarios.
    /// </summary>
    /// <remarks>
    /// This factory demonstrates how design-time factories can be implemented through inheritance
    /// rather than direct interface implementation, which is useful for testing the
    /// <see cref="EdmxConverter.ImplementsDesignTimeFactory"/> method.
    /// </remarks>
    public class CustomDbContextFactory : MigrationDbContextFactory
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDbContextFactory"/> class.
        /// </summary>
        public CustomDbContextFactory() : base()
        {
        }

        /// <summary>
        /// Creates a <see cref="TestDbContext"/> instance with the specified arguments.
        /// </summary>
        /// <param name="args">The arguments passed to the factory.</param>
        /// <returns>A configured <see cref="TestDbContext"/> instance.</returns>
        public override TestDbContext CreateDbContext(string[] args) => new(new DbContextOptions<TestDbContext>());

    }

}
