using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Unit tests for self-referencing relationship handling in EF Core to EDMX conversion.
    /// </summary>
    /// <remarks>
    /// These tests verify that self-referencing relationships (hierarchical structures)
    /// are properly detected, converted, and represented in the generated EDMX model.
    /// The tests use a Part entity that demonstrates parent-child relationships within
    /// the same table, which is a common pattern for bill-of-materials, organizational
    /// hierarchies, and other tree-like data structures.
    /// </remarks>
    [TestClass]
    public class SelfReferencingRelationshipTests
    {

        #region Fields

        private TestDbContext _context;
        private EdmxModelBuilder _modelBuilder;

        #endregion

        #region Test Setup and Cleanup

        /// <summary>
        /// Initializes test fixtures before each test method execution.
        /// </summary>
        /// <remarks>
        /// Sets up an in-memory database context and initializes the EDMX model builder
        /// to ensure each test runs with a clean, isolated environment.
        /// </remarks>
        [TestInitialize]
        public async Task TestInitialize()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
                .Options;

            _context = new TestDbContext(options);
            await _context.Database.EnsureCreatedAsync();

            _modelBuilder = new EdmxModelBuilder();
        }

        /// <summary>
        /// Cleans up test fixtures after each test method execution.
        /// </summary>
        /// <remarks>
        /// Disposes of the database context and performs cleanup to prevent
        /// resource leaks and ensure test isolation.
        /// </remarks>
        [TestCleanup]
        public async Task TestCleanup()
        {
            if (_context is not null)
            {
                await _context.Database.EnsureDeletedAsync();
                await _context.DisposeAsync();
            }
        }

        #endregion

        #region Input Validation Tests

        [TestMethod]
        public void BuildEdmxModel_WithNullModel_ShouldThrowArgumentNullException()
        {
            Action act = () => _modelBuilder.BuildEdmxModel(null);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("efModel");
        }

        #endregion

        #region Self-Referencing Relationship Detection Tests

        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingEntity_ShouldDetectSelfReferencialRelationship()
        {
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            edmxModel.Should().NotBeNull();
            edmxModel.Associations.Should().NotBeEmpty();

            // Find the self-referencing association for Parts
            var partAssociation = edmxModel.Associations
                .FirstOrDefault(a => a.Name.Contains("Part") && 
                                   a.End1.Type.Contains("Part") && 
                                   a.End2.Type.Contains("Part"));

            partAssociation.Should().NotBeNull("Should detect self-referencing relationship in Parts entity");
            partAssociation.End1.Type.Should().Contain("Part");
            partAssociation.End2.Type.Should().Contain("Part");
        }

        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingEntity_ShouldGenerateUniqueRoleNames()
        {
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            var partAssociation = edmxModel.Associations
                .FirstOrDefault(a => a.Name.Contains("Part") && 
                                   a.End1.Type.Contains("Part") && 
                                   a.End2.Type.Contains("Part"));

            partAssociation.Should().NotBeNull();
            partAssociation.End1.Role.Should().NotBe(partAssociation.End2.Role, 
                "Self-referencing relationships should have unique role names");
            
            // Verify role names indicate parent/child relationship with new semantic naming
            var roleNames = new[] { partAssociation.End1.Role, partAssociation.End2.Role };
            roleNames.Should().Contain(role => role.Contains("Parent"));
            roleNames.Should().Contain(role => role.Contains("Children"));
        }

        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingEntity_ShouldSetCorrectMultiplicity()
        {
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            var partAssociation = edmxModel.Associations
                .FirstOrDefault(a => a.Name.Contains("Part") && 
                                   a.End1.Type.Contains("Part") && 
                                   a.End2.Type.Contains("Part"));

            partAssociation.Should().NotBeNull();

            // One end should be 0..1 (parent can be null) and the other should be * (many children)
            var multiplicities = new[] { partAssociation.End1.Multiplicity, partAssociation.End2.Multiplicity };
            multiplicities.Should().Contain("0..1", "Parent relationship should be optional (0..1)");
            multiplicities.Should().Contain("*", "Child relationship should be many (*)");
        }

        #endregion

        #region Navigation Property Tests

        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingEntity_ShouldGenerateNavigationProperties()
        {
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            var partEntity = edmxModel.EntityTypes.FirstOrDefault(e => e.Name == "Part");
            partEntity.Should().NotBeNull();

            var navigationProperties = partEntity.NavigationProperties;
            navigationProperties.Should().NotBeEmpty("Part entity should have navigation properties");

            // Should have both parent and child navigation properties with improved semantic names
            navigationProperties.Should().Contain(np => np.Name == "Parent", 
                "Should have navigation to parent part");
            navigationProperties.Should().Contain(np => np.Name == "Children", 
                "Should have navigation to child parts");
        }

        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingEntity_ShouldSetCorrectNavigationPropertyRoles()
        {
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            var partEntity = edmxModel.EntityTypes.FirstOrDefault(e => e.Name == "Part");
            var parentNavigation = partEntity.NavigationProperties.FirstOrDefault(np => np.Name == "Parent");
            var childNavigation = partEntity.NavigationProperties.FirstOrDefault(np => np.Name == "Children");

            parentNavigation.Should().NotBeNull();
            childNavigation.Should().NotBeNull();

            // Navigation properties should reference the same relationship but with different roles
            parentNavigation.Relationship.Should().Be(childNavigation.Relationship,
                "Both navigation properties should reference the same association");

            parentNavigation.FromRole.Should().NotBe(parentNavigation.ToRole,
                "FromRole and ToRole should be different for parent navigation");
            childNavigation.FromRole.Should().NotBe(childNavigation.ToRole,
                "FromRole and ToRole should be different for child navigation");
        }

        #endregion

        #region Referential Constraint Tests

        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingEntity_ShouldGenerateReferentialConstraint()
        {
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            var partAssociation = edmxModel.Associations
                .FirstOrDefault(a => a.Name.Contains("Part") && 
                                   a.End1.Type.Contains("Part") && 
                                   a.End2.Type.Contains("Part"));

            partAssociation.Should().NotBeNull();
            partAssociation.ReferentialConstraint.Should().NotBeNull(
                "Self-referencing relationship should have referential constraint");

            var constraint = partAssociation.ReferentialConstraint;
            constraint.Principal.Should().NotBeNull();
            constraint.Dependent.Should().NotBeNull();

            // Principal should reference Id, dependent should reference ParentId
            constraint.Principal.PropertyRefs.Should().Contain("Id",
                "Principal role should reference the Id property");
            constraint.Dependent.PropertyRefs.Should().Contain("ParentId",
                "Dependent role should reference the ParentId foreign key property");
        }

        #endregion

        #region Entity Property Tests

        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingEntity_ShouldIncludeAllProperties()
        {
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            var partEntity = edmxModel.EntityTypes.FirstOrDefault(e => e.Name == "Part");
            partEntity.Should().NotBeNull();

            var properties = partEntity.Properties;
            
            // Verify all expected properties are present
            properties.Should().Contain(p => p.Name == "Id");
            properties.Should().Contain(p => p.Name == "ParentId");
            properties.Should().Contain(p => p.Name == "ManufacturerLocationId");
            properties.Should().Contain(p => p.Name == "InternalId");
            properties.Should().Contain(p => p.Name == "DisplayName");
            properties.Should().Contain(p => p.Name == "UniversalProductCode");
            properties.Should().Contain(p => p.Name == "Description");
            properties.Should().Contain(p => p.Name == "DateCreated");
            properties.Should().Contain(p => p.Name == "CreatedById");
            properties.Should().Contain(p => p.Name == "DateUpdated");
            properties.Should().Contain(p => p.Name == "UpdatedById");
        }

        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingEntity_ShouldSetCorrectPropertyNullability()
        {
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            var partEntity = edmxModel.EntityTypes.FirstOrDefault(e => e.Name == "Part");
            var parentIdProperty = partEntity.Properties.FirstOrDefault(p => p.Name == "ParentId");
            var displayNameProperty = partEntity.Properties.FirstOrDefault(p => p.Name == "DisplayName");

            parentIdProperty.Should().NotBeNull();
            parentIdProperty.Nullable.Should().BeTrue("ParentId should be nullable to allow root-level parts");

            displayNameProperty.Should().NotBeNull();
            displayNameProperty.Nullable.Should().BeFalse("DisplayName should be required");
        }

        #endregion

        #region Complex Hierarchy Tests

        [TestMethod]
        public async Task BuildEdmxModel_WithMultipleLevelHierarchy_ShouldHandleDeepRelationships()
        {
            // Create a multi-level hierarchy: RootPart -> SubAssembly -> Component
            var rootPart = new Part
            {
                Id = Guid.NewGuid(),
                DisplayName = "Root Assembly",
                DateCreated = DateTimeOffset.UtcNow,
                CreatedById = Guid.NewGuid(),
                UpdatedById = Guid.NewGuid(),
                ManufacturerLocationId = Guid.NewGuid()
            };

            var subAssembly = new Part
            {
                Id = Guid.NewGuid(),
                ParentId = rootPart.Id,
                DisplayName = "Sub Assembly",
                DateCreated = DateTimeOffset.UtcNow,
                CreatedById = Guid.NewGuid(),
                UpdatedById = Guid.NewGuid(),
                ManufacturerLocationId = Guid.NewGuid()
            };

            var component = new Part
            {
                Id = Guid.NewGuid(),
                ParentId = subAssembly.Id,
                DisplayName = "Component",
                DateCreated = DateTimeOffset.UtcNow,
                CreatedById = Guid.NewGuid(),
                UpdatedById = Guid.NewGuid(),
                ManufacturerLocationId = Guid.NewGuid()
            };

            _context.Parts.AddRange(rootPart, subAssembly, component);
            await _context.SaveChangesAsync();

            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            // The relationship structure should be properly represented regardless of data depth
            var partAssociation = edmxModel.Associations
                .FirstOrDefault(a => a.Name.Contains("Part") && 
                                   a.End1.Type.Contains("Part") && 
                                   a.End2.Type.Contains("Part"));

            partAssociation.Should().NotBeNull("Should handle multi-level hierarchies");
            partAssociation.ReferentialConstraint.Should().NotBeNull();
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public async Task ConvertToEdmx_WithSelfReferencingRelationship_ShouldGenerateValidXml()
        {
            var converter = new EdmxConverter();

            await _context.Database.EnsureCreatedAsync();

            var result = converter.ConvertToEdmx(_context);

            result.Should().NotBeNull();
            result.DbContextName.Should().Be("TestDbContext");
            result.EdmxContent.Should().NotBeNullOrEmpty("Should generate EDMX content");

            // Verify the XML contains self-referencing relationship elements
            result.EdmxContent.Should().Contain("Part", "Generated EDMX should contain Part entity");
            result.EdmxContent.Should().Contain("Association", "Generated EDMX should contain associations");
            result.EdmxContent.Should().Contain("NavigationProperty", "Generated EDMX should contain navigation properties");
        }

        #endregion

        #region Improved Navigation Property Naming Tests

        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingEntity_ShouldUseImprovedNavigationPropertyNames()
        {
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            var partEntity = edmxModel.EntityTypes.FirstOrDefault(e => e.Name == "Part");
            partEntity.Should().NotBeNull();

            var navigationProperties = partEntity.NavigationProperties;
            
            // Verify the improved semantic navigation property names
            var parentNavigation = navigationProperties.FirstOrDefault(np => np.Name == "Parent");
            var childrenNavigation = navigationProperties.FirstOrDefault(np => np.Name == "Children");

            parentNavigation.Should().NotBeNull("Should have Parent navigation property with improved naming");
            childrenNavigation.Should().NotBeNull("Should have Children navigation property with improved naming");

            // Verify old confusing names are not present
            navigationProperties.Should().NotContain(np => np.Name == "InverseParent", 
                "Should not contain confusing EF Core default name 'InverseParent'");
            navigationProperties.Should().NotContain(np => np.Name == "ParentPart", 
                "Should not contain old naming convention 'ParentPart'");
        }

        [TestMethod]
        public void ConvertToEdmx_WithSelfReferencingEntity_ShouldGenerateImprovedNavigationNamesInXml()
        {
            var converter = new EdmxConverter();
            var result = converter.ConvertToEdmx(_context);

            result.Should().NotBeNull();
            result.EdmxContent.Should().NotBeNullOrEmpty();

            // Verify the improved navigation property names appear in the generated XML
            result.EdmxContent.Should().Contain("NavigationProperty Name=\"Parent\"", 
                "Generated XML should contain Parent navigation with improved naming");
            result.EdmxContent.Should().Contain("NavigationProperty Name=\"Children\"", 
                "Generated XML should contain Children navigation with improved naming");

            // Verify old confusing names are not in the XML
            result.EdmxContent.Should().NotContain("NavigationProperty Name=\"InverseParent\"", 
                "Generated XML should not contain confusing EF Core default name");
            result.EdmxContent.Should().NotContain("NavigationProperty Name=\"ParentPart\"", 
                "Generated XML should not contain old naming convention");
        }

        #endregion

        #region Edge Case Tests

        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingEntity_ShouldHandleNullNavigationPropertyNames()
        {
            // This tests the fallback behavior when navigation properties don't have explicit names
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            // Should not throw exception and should generate reasonable default names
            var partAssociation = edmxModel.Associations
                .FirstOrDefault(a => a.Name.Contains("Part") && 
                                   a.End1.Type.Contains("Part") && 
                                   a.End2.Type.Contains("Part"));

            partAssociation.Should().NotBeNull();
            partAssociation.End1.Role.Should().NotBeNullOrEmpty();
            partAssociation.End2.Role.Should().NotBeNullOrEmpty();
            partAssociation.End1.Role.Should().NotBe(partAssociation.End2.Role);
        }

        [TestMethod]
        public void BuildEdmxModel_WithSelfReferencingEntity_ShouldGenerateUniqueAssociationName()
        {
            var model = _context.Model;

            var edmxModel = _modelBuilder.BuildEdmxModel(model);

            var partAssociation = edmxModel.Associations
                .FirstOrDefault(a => a.Name.Contains("Part") && 
                                   a.End1.Type.Contains("Part") && 
                                   a.End2.Type.Contains("Part"));

            partAssociation.Should().NotBeNull();
            partAssociation.Name.Should().NotBeNullOrEmpty("Association should have a name");
            
            // Name should be unique and descriptive for self-referencing relationships
            partAssociation.Name.Should().Contain("Part", "Association name should reference the entity");
            
            // Verify no duplicate association names exist
            var associationNames = edmxModel.Associations.Select(a => a.Name).ToList();
            associationNames.Should().OnlyHaveUniqueItems("All association names should be unique");
        }

        #endregion

    }

}