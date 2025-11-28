using Microsoft.EntityFrameworkCore;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models
{
    /// <summary>
    /// Test database context used for unit testing EF Core to EDMX conversion functionality.
    /// </summary>
    /// <remarks>
    /// This context defines a simple but comprehensive data model that includes various
    /// entity relationships, property types, and metadata configurations for testing
    /// all aspects of the EDMX conversion process. It includes examples of one-to-many
    /// relationships, foreign keys, computed properties, and documentation annotations.
    /// </remarks>
    public class TestDbContext : DbContext
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDbContext"/> class.
        /// </summary>
        public TestDbContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to configure the context.</param>
        /// <remarks>
        /// Constructor accepts DbContext options to support both in-memory and
        /// real database connections for testing different scenarios.
        /// </remarks>
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {

        }

        /// <summary>
        /// Gets or sets the collection of users in the database.
        /// </summary>
        /// <value>
        /// A <see cref="DbSet{User}"/> representing the Users table.
        /// </value>
        /// <remarks>
        /// The Users entity set serves as the principal side of relationships
        /// with Orders and demonstrates basic entity configuration.
        /// </remarks>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the collection of orders in the database.
        /// </summary>
        /// <value>
        /// A <see cref="DbSet{Order}"/> representing the Orders table.
        /// </value>
        /// <remarks>
        /// The Orders entity set demonstrates foreign key relationships to Users
        /// and serves as the principal side of the relationship with OrderItems.
        /// </remarks>
        public DbSet<Order> Orders { get; set; }

        /// <summary>
        /// Gets or sets the collection of order items in the database.
        /// </summary>
        /// <value>
        /// A <see cref="DbSet{OrderItem}"/> representing the OrderItems table.
        /// </value>
        /// <remarks>
        /// The OrderItems entity set demonstrates dependent relationships and
        /// foreign key configurations to Orders.
        /// </remarks>
        public DbSet<OrderItem> OrderItems { get; set; }

        /// <summary>
        /// Gets or sets the collection of parts in the database.
        /// </summary>
        /// <value>
        /// A <see cref="DbSet{Part}"/> representing the Parts table.
        /// </value>
        /// <remarks>
        /// The Parts entity set demonstrates self-referencing relationships where
        /// parts can be components of other parts, creating hierarchical structures.
        /// </remarks>
        public DbSet<Part> Parts { get; set; }

        /// <summary>
        /// Gets or sets the collection of NAICS codes in the database.
        /// </summary>
        /// <value>
        /// A <see cref="DbSet{NaicsCode}"/> representing the NaicsCodes table.
        /// </value>
        /// <remarks>
        /// The NaicsCodes entity set demonstrates PostgreSQL ltree type handling
        /// for hierarchical classification codes.
        /// </remarks>
        public DbSet<NaicsCode> NaicsCodes { get; set; }

        /// <summary>
        /// Configures the model and entity relationships using Fluent API.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure the context.</param>
        /// <remarks>
        /// This method demonstrates various EF Core configurations including:
        /// - Primary key definitions
        /// - Property constraints (max length, precision, scale)
        /// - Foreign key relationships with cascade behavior
        /// - Default value configurations
        /// - Documentation comments for property metadata
        /// - Index definitions
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable(tb => tb.HasComment("Represents a User of the system."));
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Add comments for documentation testing
                entity.Property(e => e.Email).HasComment("User's email address");
                entity.Property(e => e.FirstName).HasComment("User's first name");

                // Add unique index on email
                entity.HasIndex(e => e.Email).IsUnique();

            });

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {

                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

                entity.HasOne(e => e.User)
                      .WithMany(e => e.Orders)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

            });

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {

                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

                entity.HasOne(e => e.Order)
                      .WithMany(e => e.OrderItems)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

            });

            // Configure Part entity (self-referencing relationship)
            modelBuilder.Entity<Part>(entity =>
            {

                entity.HasKey(e => e.Id);
                
                // Configure string properties with appropriate constraints
                entity.Property(e => e.DisplayName).IsRequired();
                entity.Property(e => e.UniversalProductCode).HasMaxLength(12);
                
                // Configure required audit fields
                entity.Property(e => e.DateCreated).IsRequired();
                entity.Property(e => e.CreatedById).IsRequired();
                entity.Property(e => e.UpdatedById).IsRequired();

                // Configure self-referencing relationship
                entity.HasOne(e => e.ParentPart)
                      .WithMany(e => e.ChildParts)
                      .HasForeignKey(e => e.ParentId)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes for hierarchical data

                // Add comments for documentation testing
                entity.Property(e => e.ParentId).HasComment("The Part ID this Part is a component of.");
                entity.Property(e => e.DisplayName).HasComment("Human-readable name for the part.");
                entity.Property(e => e.UniversalProductCode).HasComment("12-character Universal Product Code.");

                // Add index on DateCreated for performance
                entity.HasIndex(e => e.DateCreated).HasDatabaseName("IX_Parts_DateCreated");

                // Add foreign key constraint name to match your schema
                entity.HasOne(e => e.ParentPart)
                      .WithMany(e => e.ChildParts)
                      .HasConstraintName("FK_Parts_ParentPart");

            });

            // Configure NaicsCode entity
            modelBuilder.Entity<NaicsCode>(entity =>
            {

                entity.HasKey(e => e.Id);

                // Configure ltree column type for PostgreSQL hierarchical path
                entity.Property(e => e.Path)
                      .HasColumnType("ltree")
                      .IsRequired();

                entity.Property(e => e.Title).HasMaxLength(500);
                entity.Property(e => e.Code).HasMaxLength(10);

                // Add comment for documentation testing
                entity.Property(e => e.Path).HasComment("Hierarchical NAICS code path using PostgreSQL ltree type");

            });

        }

    }

}
