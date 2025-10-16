using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Tests for self-referencing relationships with column name mappings.
    /// </summary>
    /// <remarks>
    /// These tests ensure that self-referencing relationships correctly preserve column names
    /// when the foreign key column has a different name in the database than the CLR property.
    /// </remarks>
    [TestClass]
    public class SelfReferencingColumnMappingTests
    {

        #region Test Entities

        /// <summary>
        /// Test entity with self-referencing relationship and column mappings.
        /// </summary>
        public class Part
        {
            public Guid Id { get; set; }

            [Column("INTERNAL_ID")]
            public string InternalId { get; set; }

            [Column("DISPLAY_NAME")]
            public string DisplayName { get; set; }

            [Column("PARENT_ID")]
            public Guid? ParentId { get; set; }

            [Column("UNIVERSAL_PRODUCT_CODE")]
            public string UniversalProductCode { get; set; }

            [Column("DATE_CREATED")]
            public DateTimeOffset DateCreated { get; set; }

            [Column("CREATED_BY_ID")]
            public Guid CreatedById { get; set; }

            [Column("DATE_UPDATED")]
            public DateTimeOffset? DateUpdated { get; set; }

            [Column("UPDATED_BY_ID")]
            public Guid? UpdatedById { get; set; }

            // Navigation properties
            public Part Parent { get; set; }
            public ICollection<Part> Children { get; set; }
        }

        /// <summary>
        /// Test DbContext with self-referencing entity and column mappings.
        /// </summary>
        public class SelfReferencingTestDbContext : DbContext
        {
            public SelfReferencingTestDbContext(DbContextOptions<SelfReferencingTestDbContext> options)
                : base(options)
            {
            }

            public DbSet<Part> Parts { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Part>(entity =>
                {
                    entity.ToTable("Parts");
                    entity.HasKey(e => e.Id);

                    // Configure properties with column mappings using HasColumnName
                    entity.Property(e => e.InternalId).HasColumnName("INTERNAL_ID").HasMaxLength(50);
                    entity.Property(e => e.DisplayName).HasColumnName("DISPLAY_NAME").HasMaxLength(255).IsRequired();
                    entity.Property(e => e.ParentId).HasColumnName("PARENT_ID");
                    entity.Property(e => e.UniversalProductCode).HasColumnName("UNIVERSAL_PRODUCT_CODE").HasMaxLength(12);
                    entity.Property(e => e.DateCreated).HasColumnName("DATE_CREATED").IsRequired();
                    entity.Property(e => e.CreatedById).HasColumnName("CREATED_BY_ID").IsRequired();
                    entity.Property(e => e.DateUpdated).HasColumnName("DATE_UPDATED");
                    entity.Property(e => e.UpdatedById).HasColumnName("UPDATED_BY_ID");

                    // Configure self-referencing relationship with mapped column
                    entity.HasOne(e => e.Parent)
                          .WithMany(e => e.Children)
                          .HasForeignKey(e => e.ParentId)
                          .OnDelete(DeleteBehavior.Restrict);

                    // Add documentation
                    entity.Property(e => e.ParentId).HasComment("Reference to the parent part");
                    entity.Property(e => e.DisplayName).HasComment("Human-readable name for the part");
                });
            }
        }

        #endregion

        #region Fields

        private EdmxModelBuilder _modelBuilder;
        private EdmxXmlGenerator _xmlGenerator;
        private SelfReferencingTestDbContext _context;

        #endregion

        #region Test Setup and Cleanup

        /// <summary>
        /// Initializes test dependencies before each test method execution.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _modelBuilder = new EdmxModelBuilder();

            var options = new DbContextOptionsBuilder<SelfReferencingTestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SelfReferencingTestDbContext(options);
        }

        /// <summary>
        /// Cleans up test resources after each test method execution.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }

        #endregion

        #region Self-Referencing Column Mapping Tests

        /// <summary>
        /// Tests that self-referencing foreign key columns preserve database column names in SSDL.
        /// </summary>
        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingAndColumnMapping_ShouldPreserveForeignKeyColumnName()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model, "TestNamespace", "TestContainer");
            
            _xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            var xmlContent = _xmlGenerator.Generate();
            var doc = XDocument.Parse(xmlContent);

            // Act - Find the Parts entity type in SSDL
            XNamespace ssdlNs = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
            var storageEntityType = doc
                .Descendants(ssdlNs + "EntityType")
                .FirstOrDefault(e => e.Attribute("Name")?.Value == "Parts");

            storageEntityType.Should().NotBeNull("Parts entity should exist in SSDL");

            // Assert - Verify the PARENT_ID column is preserved
            var properties = storageEntityType.Elements(ssdlNs + "Property").ToList();
            
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "PARENT_ID",
                "PARENT_ID column should be uppercase in SSDL");
            
            // Also check other mapped columns
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "INTERNAL_ID");
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "DISPLAY_NAME");
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "UNIVERSAL_PRODUCT_CODE");
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "DATE_CREATED");
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "CREATED_BY_ID");
        }

        /// <summary>
        /// Tests that self-referencing associations use correct column names in referential constraints.
        /// </summary>
        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingAssociation_ShouldUseCorrectColumnNamesInConstraints()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model, "TestNamespace", "TestContainer");
            
            _xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            var xmlContent = _xmlGenerator.Generate();
            var doc = XDocument.Parse(xmlContent);

            // Act - Find the self-referencing association in SSDL
            XNamespace ssdlNs = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
            var associations = doc
                .Descendants(ssdlNs + "Association")
                .Where(a => a.Attribute("Name")?.Value.Contains("Part_Parent_Children") == true)
                .ToList();

            associations.Should().NotBeEmpty("Self-referencing association should exist");

            var association = associations.First();
            var referentialConstraint = association.Element(ssdlNs + "ReferentialConstraint");
            referentialConstraint.Should().NotBeNull("Referential constraint should exist");

            // Assert - Check that the dependent property uses PARENT_ID
            var dependentPropertyRef = referentialConstraint
                .Element(ssdlNs + "Dependent")
                ?.Element(ssdlNs + "PropertyRef");

            dependentPropertyRef.Should().NotBeNull();
            dependentPropertyRef.Attribute("Name")?.Value.Should().Be("PARENT_ID",
                "Dependent property should reference PARENT_ID column");

            // Check principal property references Id
            var principalPropertyRef = referentialConstraint
                .Element(ssdlNs + "Principal")
                ?.Element(ssdlNs + "PropertyRef");

            principalPropertyRef.Should().NotBeNull();
            principalPropertyRef.Attribute("Name")?.Value.Should().Be("Id",
                "Principal property should reference Id column");
        }

        /// <summary>
        /// Tests that self-referencing relationships have correct role names in storage model.
        /// </summary>
        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencing_ShouldGenerateUniqueRoleNamesInStorageModel()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model, "TestNamespace", "TestContainer");
            
            _xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            var xmlContent = _xmlGenerator.Generate();
            var doc = XDocument.Parse(xmlContent);

            // Act - Find the self-referencing association in SSDL
            XNamespace ssdlNs = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
            var association = doc
                .Descendants(ssdlNs + "Association")
                .FirstOrDefault(a => a.Attribute("Name")?.Value.Contains("Part_Parent_Children") == true);

            association.Should().NotBeNull("Self-referencing association should exist");

            var ends = association.Elements(ssdlNs + "End").ToList();

            // Assert - Verify unique role names are generated
            ends.Should().HaveCount(2, "Association should have exactly 2 ends");

            var roles = ends.Select(e => e.Attribute("Role")?.Value).ToList();
            roles.Should().Contain("Parts_Principal", "Should have Parts_Principal role");
            roles.Should().Contain("Parts_Dependent", "Should have Parts_Dependent role");

            // Both ends should reference the same entity type (Parts)
            ends.Should().AllSatisfy(e => 
                e.Attribute("Type")?.Value.Should().Be("Self.Parts",
                    "Both ends should reference the Parts entity type"));
        }

        /// <summary>
        /// Tests that mappings correctly connect CLR properties to database columns for self-referencing relationships.
        /// </summary>
        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencing_ShouldCreateCorrectPropertyMappings()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model, "TestNamespace", "TestContainer");
            
            _xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            var xmlContent = _xmlGenerator.Generate();
            var doc = XDocument.Parse(xmlContent);

            // Act - Find the mappings for Parts
            XNamespace mappingNs = "http://schemas.microsoft.com/ado/2009/11/mapping/cs";
            var entitySetMapping = doc
                .Descendants(mappingNs + "EntitySetMapping")
                .FirstOrDefault(e => e.Attribute("Name")?.Value == "Parts");

            entitySetMapping.Should().NotBeNull("Parts entity set mapping should exist");

            var scalarProperties = entitySetMapping
                .Descendants(mappingNs + "ScalarProperty")
                .ToList();

            // Assert - Verify mappings for self-referencing foreign key
            var parentIdMapping = scalarProperties.FirstOrDefault(p => p.Attribute("Name")?.Value == "ParentId");
            parentIdMapping.Should().NotBeNull("ParentId mapping should exist");
            parentIdMapping.Attribute("ColumnName")?.Value.Should().Be("PARENT_ID",
                "ParentId property should map to PARENT_ID column");

            // Verify other column mappings
            var displayNameMapping = scalarProperties.FirstOrDefault(p => p.Attribute("Name")?.Value == "DisplayName");
            displayNameMapping.Should().NotBeNull();
            displayNameMapping.Attribute("ColumnName")?.Value.Should().Be("DISPLAY_NAME",
                "DisplayName property should map to DISPLAY_NAME column");

            var dateCreatedMapping = scalarProperties.FirstOrDefault(p => p.Attribute("Name")?.Value == "DateCreated");
            dateCreatedMapping.Should().NotBeNull();
            dateCreatedMapping.Attribute("ColumnName")?.Value.Should().Be("DATE_CREATED",
                "DateCreated property should map to DATE_CREATED column");
        }

        /// <summary>
        /// Tests that navigation properties for self-referencing relationships are correctly named.
        /// </summary>
        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencing_ShouldHaveCorrectNavigationPropertyNames()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model, "TestNamespace", "TestContainer");

            // Act
            var partEntity = edmxModel.EntityTypes.FirstOrDefault(e => e.Name == "Part");
            partEntity.Should().NotBeNull("Part entity should exist");

            var navigationProperties = partEntity.NavigationProperties;

            // Assert - Should have Parent and Children navigation properties
            navigationProperties.Should().Contain(np => np.Name == "Parent",
                "Should have Parent navigation property");
            navigationProperties.Should().Contain(np => np.Name == "Children",
                "Should have Children navigation property");

            // Verify the navigation properties reference the correct relationship
            var parentNav = navigationProperties.FirstOrDefault(np => np.Name == "Parent");
            parentNav.Should().NotBeNull();
            parentNav.Relationship.Should().Contain("Part_Parent_Children",
                "Parent navigation should reference the self-referencing relationship");

            var childrenNav = navigationProperties.FirstOrDefault(np => np.Name == "Children");
            childrenNav.Should().NotBeNull();
            childrenNav.Relationship.Should().Contain("Part_Parent_Children",
                "Children navigation should reference the self-referencing relationship");
        }

        #endregion

    }

}