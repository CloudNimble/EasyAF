using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Regression tests for column name mapping between CLR properties and database columns.
    /// </summary>
    /// <remarks>
    /// These tests ensure that the EDMX generator correctly preserves actual database column names
    /// in the SSDL (Storage Schema Definition Language) while using CLR property names in the 
    /// CSDL (Conceptual Schema Definition Language). This addresses the issue where column names
    /// like "NIIN", "FSC" were being incorrectly converted to "Niin", "Fsc" in the storage model.
    /// </remarks>
    [TestClass]
    public class ColumnNameMappingTests
    {

        #region Test DbContext with Column Mappings

        /// <summary>
        /// Test entity with uppercase column names in the database.
        /// </summary>
        public class NationalStockNumber
        {
            public Guid Id { get; set; }

            [Column("NIIN")]
            public string Niin { get; set; }

            [Column("FSC")]
            public string Fsc { get; set; }

            [Column("INC")]
            public string Inc { get; set; }

            [Column("SOS")]
            public string Sos { get; set; }

            public string DisplayName { get; set; }

            [Column("EndItemDisplayName")]
            public string EndItemDisplayName { get; set; }
        }

        /// <summary>
        /// Test entity with mixed case column names.
        /// </summary>
        public class FederalSupplyClass
        {
            public Guid Id { get; set; }

            [Column("Code")]
            public string Code { get; set; }

            [Column("DisplayName")]
            public string DisplayName { get; set; }

            [Column("FederalSupplyGroupId")]
            public Guid FederalSupplyGroupId { get; set; }

            public FederalSupplyGroup FederalSupplyGroup { get; set; }
        }

        /// <summary>
        /// Test entity for foreign key relationships.
        /// </summary>
        public class FederalSupplyGroup
        {
            public Guid Id { get; set; }

            [Column("Code")]
            public string Code { get; set; }

            [Column("DisplayName")]
            public string DisplayName { get; set; }

            public ICollection<FederalSupplyClass> FederalSupplyClasses { get; set; }
        }

        /// <summary>
        /// Test DbContext with column name mappings.
        /// </summary>
        public class ColumnMappingTestDbContext : DbContext
        {
            public ColumnMappingTestDbContext(DbContextOptions<ColumnMappingTestDbContext> options) 
                : base(options)
            {
            }

            public DbSet<NationalStockNumber> NationalStockNumbers { get; set; }
            public DbSet<FederalSupplyClass> FederalSupplyClasses { get; set; }
            public DbSet<FederalSupplyGroup> FederalSupplyGroups { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Configure NationalStockNumber with uppercase columns
                modelBuilder.Entity<NationalStockNumber>(entity =>
                {
                    entity.ToTable("NationalStockNumbers");
                    entity.HasKey(e => e.Id);
                    
                    // Use HasColumnName to explicitly set database column names
                    // This works with all providers including InMemory for testing
                    entity.Property(e => e.Niin).HasColumnName("NIIN").HasMaxLength(9).IsRequired();
                    entity.Property(e => e.Fsc).HasColumnName("FSC").HasMaxLength(4).IsRequired();
                    entity.Property(e => e.Inc).HasColumnName("INC").HasMaxLength(5);
                    entity.Property(e => e.Sos).HasColumnName("SOS").HasMaxLength(20);
                    entity.Property(e => e.DisplayName).HasMaxLength(255);
                    entity.Property(e => e.EndItemDisplayName).HasColumnName("EndItemDisplayName").HasMaxLength(1000);
                });

                // Configure FederalSupplyClass
                modelBuilder.Entity<FederalSupplyClass>(entity =>
                {
                    entity.ToTable("FederalSupplyClasses");
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Code).HasColumnName("Code").HasMaxLength(4).IsRequired();
                    entity.Property(e => e.DisplayName).HasColumnName("DisplayName").HasMaxLength(255).IsRequired();
                    entity.Property(e => e.FederalSupplyGroupId).HasColumnName("FederalSupplyGroupId");
                });

                // Configure FederalSupplyGroup
                modelBuilder.Entity<FederalSupplyGroup>(entity =>
                {
                    entity.ToTable("FederalSupplyGroups");
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Code).HasColumnName("Code").HasMaxLength(2).IsRequired();
                    entity.Property(e => e.DisplayName).HasColumnName("DisplayName").HasMaxLength(255).IsRequired();
                });

                // Configure relationship
                modelBuilder.Entity<FederalSupplyClass>()
                    .HasOne(e => e.FederalSupplyGroup)
                    .WithMany(e => e.FederalSupplyClasses)
                    .HasForeignKey(e => e.FederalSupplyGroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }

        #endregion

        #region Fields

        private EdmxModelBuilder _modelBuilder;
        private EdmxXmlGenerator _xmlGenerator;
        private ColumnMappingTestDbContext _context;

        #endregion

        #region Test Setup and Cleanup

        /// <summary>
        /// Initializes test dependencies before each test method execution.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _modelBuilder = new EdmxModelBuilder();

            var options = new DbContextOptionsBuilder<ColumnMappingTestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ColumnMappingTestDbContext(options);
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

        #region Column Name Mapping Tests

        /// <summary>
        /// Tests that uppercase column names (NIIN, FSC, etc.) are preserved in the SSDL.
        /// </summary>
        /// <remarks>
        /// This is a regression test for the issue where column names like "NIIN" were being
        /// incorrectly converted to "Niin" in the storage model.
        /// </remarks>
        [TestMethod]
        public void BuildEdmxModel_WithUppercaseColumnNames_ShouldPreserveColumnNamesInStorageModel()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model, "TestNamespace", "TestContainer");
            
            _xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            var xmlContent = _xmlGenerator.Generate();
            var doc = XDocument.Parse(xmlContent);

            // Act - Find the NationalStockNumbers entity type in SSDL
            XNamespace ssdlNs = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
            var storageModels = doc.Descendants(XName.Get("StorageModels", "http://schemas.microsoft.com/ado/2009/11/edmx")).FirstOrDefault();
            storageModels.Should().NotBeNull("SSDL section should exist");

            var storageEntityType = storageModels
                .Descendants(ssdlNs + "EntityType")
                .FirstOrDefault(e => e.Attribute("Name")?.Value == "NationalStockNumbers");

            storageEntityType.Should().NotBeNull("NationalStockNumbers entity should exist in SSDL");

            // Assert - Verify uppercase column names are preserved in SSDL
            var properties = storageEntityType.Elements(ssdlNs + "Property").ToList();

            // Check that uppercase columns are preserved
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "NIIN",
                "NIIN column should be uppercase in SSDL");
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "FSC",
                "FSC column should be uppercase in SSDL");
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "INC",
                "INC column should be uppercase in SSDL");
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "SOS",
                "SOS column should be uppercase in SSDL");

            // Also verify the property still exists (not converted)
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "DisplayName");
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "EndItemDisplayName");
        }

        /// <summary>
        /// Tests that CLR property names are used in the CSDL while database column names are used in SSDL.
        /// </summary>
        [TestMethod]
        public void BuildEdmxModel_WithColumnMapping_ShouldUseCLRNamesInConceptualModel()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model, "TestNamespace", "TestContainer");
            
            _xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            var xmlContent = _xmlGenerator.Generate();
            var doc = XDocument.Parse(xmlContent);

            // Act - Find the NationalStockNumber entity type in CSDL
            XNamespace edmNs = "http://schemas.microsoft.com/ado/2009/11/edm";
            var conceptualModels = doc.Descendants(XName.Get("ConceptualModels", "http://schemas.microsoft.com/ado/2009/11/edmx")).FirstOrDefault();
            conceptualModels.Should().NotBeNull("CSDL section should exist");

            var conceptualEntityType = conceptualModels
                .Descendants(edmNs + "EntityType")
                .FirstOrDefault(e => e.Attribute("Name")?.Value == "NationalStockNumber");

            conceptualEntityType.Should().NotBeNull("NationalStockNumber entity should exist in CSDL");

            // Assert - Verify CLR property names are used in CSDL
            var properties = conceptualEntityType.Elements(edmNs + "Property").ToList();

            // Check that CLR property names (TitleCase) are used
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "Niin",
                "Niin property should use CLR name in CSDL");
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "Fsc",
                "Fsc property should use CLR name in CSDL");
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "Inc",
                "Inc property should use CLR name in CSDL");
            properties.Should().Contain(p => p.Attribute("Name") != null && p.Attribute("Name").Value == "Sos",
                "Sos property should use CLR name in CSDL");
        }

        /// <summary>
        /// Tests that the mapping section correctly maps CLR property names to database column names.
        /// </summary>
        [TestMethod]
        public void BuildEdmxModel_WithColumnMapping_ShouldCreateCorrectMappings()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model, "TestNamespace", "TestContainer");
            
            _xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            var xmlContent = _xmlGenerator.Generate();
            var doc = XDocument.Parse(xmlContent);

            // Act - Find the mappings section
            XNamespace mappingNs = "http://schemas.microsoft.com/ado/2009/11/mapping/cs";
            var mappings = doc.Descendants(XName.Get("Mappings", "http://schemas.microsoft.com/ado/2009/11/edmx")).FirstOrDefault();
            mappings.Should().NotBeNull("Mappings section should exist");

            var entitySetMapping = mappings
                .Descendants(mappingNs + "EntitySetMapping")
                .FirstOrDefault(e => e.Attribute("Name")?.Value == "NationalStockNumbers");

            entitySetMapping.Should().NotBeNull("NationalStockNumbers entity set mapping should exist");

            var scalarProperties = entitySetMapping
                .Descendants(mappingNs + "ScalarProperty")
                .ToList();

            // Assert - Verify mappings connect CLR names to database column names
            var niinMapping = scalarProperties.FirstOrDefault(p => p.Attribute("Name")?.Value == "Niin");
            niinMapping.Should().NotBeNull();
            niinMapping.Attribute("ColumnName")?.Value.Should().Be("NIIN",
                "Niin property should map to NIIN column");

            var fscMapping = scalarProperties.FirstOrDefault(p => p.Attribute("Name")?.Value == "Fsc");
            fscMapping.Should().NotBeNull();
            fscMapping.Attribute("ColumnName")?.Value.Should().Be("FSC",
                "Fsc property should map to FSC column");

            var incMapping = scalarProperties.FirstOrDefault(p => p.Attribute("Name")?.Value == "Inc");
            incMapping.Should().NotBeNull();
            incMapping.Attribute("ColumnName")?.Value.Should().Be("INC",
                "Inc property should map to INC column");

            var sosMapping = scalarProperties.FirstOrDefault(p => p.Attribute("Name")?.Value == "Sos");
            sosMapping.Should().NotBeNull();
            sosMapping.Attribute("ColumnName")?.Value.Should().Be("SOS",
                "Sos property should map to SOS column");
        }

        /// <summary>
        /// Tests that foreign key column names are correctly mapped in associations.
        /// </summary>
        [TestMethod]
        public void BuildEdmxModel_WithForeignKeyColumnMapping_ShouldPreserveColumnNamesInAssociations()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model, "TestNamespace", "TestContainer");
            
            _xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            var xmlContent = _xmlGenerator.Generate();
            var doc = XDocument.Parse(xmlContent);

            // Act - Find associations in SSDL
            XNamespace ssdlNs = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
            var storageModels = doc.Descendants(XName.Get("StorageModels", "http://schemas.microsoft.com/ado/2009/11/edmx")).FirstOrDefault();
            storageModels.Should().NotBeNull("SSDL section should exist");

            var associations = storageModels
                .Descendants(ssdlNs + "Association")
                .ToList();

            associations.Should().NotBeEmpty("Associations should exist in SSDL");

            // Find the specific association for FederalSupplyClass -> FederalSupplyGroup
            var association = associations.FirstOrDefault(a => 
                a.Descendants(ssdlNs + "PropertyRef")
                 .Any(pr => pr.Attribute("Name")?.Value == "FederalSupplyGroupId"));

            // Assert - Verify the foreign key column name is preserved
            association.Should().NotBeNull("FederalSupplyClass association should exist");
            
            var dependentPropertyRef = association
                .Descendants(ssdlNs + "Dependent")
                .FirstOrDefault()
                ?.Descendants(ssdlNs + "PropertyRef")
                .FirstOrDefault();

            dependentPropertyRef.Should().NotBeNull();
            dependentPropertyRef.Attribute("Name")?.Value.Should().Be("FederalSupplyGroupId",
                "Foreign key column name should be preserved in association");
        }

        /// <summary>
        /// Tests that properties without explicit Column attributes use the CLR property name.
        /// </summary>
        [TestMethod]
        public void BuildEdmxModel_WithoutColumnAttribute_ShouldUseCLRPropertyName()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model, "TestNamespace", "TestContainer");
            
            _xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            var xmlContent = _xmlGenerator.Generate();
            var doc = XDocument.Parse(xmlContent);

            // Act - Find DisplayName property in both CSDL and SSDL
            XNamespace ssdlNs = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
            XNamespace edmNs = "http://schemas.microsoft.com/ado/2009/11/edm";

            var storageEntityType = doc
                .Descendants(ssdlNs + "EntityType")
                .FirstOrDefault(e => e.Attribute("Name")?.Value == "NationalStockNumbers");

            var conceptualEntityType = doc
                .Descendants(edmNs + "EntityType")
                .FirstOrDefault(e => e.Attribute("Name")?.Value == "NationalStockNumber");

            // Assert - Both should use "DisplayName" since no Column attribute was specified
            var storageProperty = storageEntityType
                ?.Elements(ssdlNs + "Property")
                .FirstOrDefault(p => p.Attribute("Name")?.Value == "DisplayName");

            var conceptualProperty = conceptualEntityType
                ?.Elements(edmNs + "Property")
                .FirstOrDefault(p => p.Attribute("Name")?.Value == "DisplayName");

            storageProperty.Should().NotBeNull("DisplayName should exist in SSDL");
            conceptualProperty.Should().NotBeNull("DisplayName should exist in CSDL");
            
            storageProperty.Attribute("Name")?.Value.Should().Be("DisplayName",
                "Properties without Column attribute should use CLR name in SSDL");
            conceptualProperty.Attribute("Name")?.Value.Should().Be("DisplayName",
                "Properties without Column attribute should use CLR name in CSDL");
        }

        /// <summary>
        /// Tests that primary key column names are correctly mapped in storage model keys.
        /// </summary>
        [TestMethod]
        public void BuildEdmxModel_WithColumnMapping_ShouldMapKeysCorrectlyInStorageModel()
        {
            // Arrange
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model, "TestNamespace", "TestContainer");
            
            // Manually set a property with column mapping as a key for testing
            var entityType = edmxModel.EntityTypes.First(e => e.Name == "NationalStockNumber");
            entityType.Keys.Clear();
            entityType.Keys.Add("Niin"); // Add Niin as a key to test column name mapping
            
            _xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            var xmlContent = _xmlGenerator.Generate();
            var doc = XDocument.Parse(xmlContent);

            // Act - Find the key in SSDL
            XNamespace ssdlNs = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
            var storageEntityType = doc
                .Descendants(ssdlNs + "EntityType")
                .FirstOrDefault(e => e.Attribute("Name")?.Value == "NationalStockNumbers");

            var keyElement = storageEntityType?.Element(ssdlNs + "Key");
            var keyPropertyRef = keyElement?.Element(ssdlNs + "PropertyRef");

            // Assert - Key should use the database column name
            keyPropertyRef.Should().NotBeNull("Key PropertyRef should exist");
            keyPropertyRef.Attribute("Name")?.Value.Should().Be("NIIN",
                "Key should reference the database column name NIIN, not the CLR property name Niin");
        }

        #endregion

        #region Edge Case Tests

        /// <summary>
        /// Tests that the system handles entities without any column mappings correctly.
        /// </summary>
        [TestMethod]
        public void BuildEdmxModel_WithNoColumnMappings_ShouldUsePropertyNamesEverywhere()
        {
            // Arrange - Use FederalSupplyGroup which has properties without Column attributes
            var model = _context.Model;
            var edmxModel = _modelBuilder.BuildEdmxModel(model, "TestNamespace", "TestContainer");
            
            _xmlGenerator = new EdmxXmlGenerator(edmxModel, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            var xmlContent = _xmlGenerator.Generate();
            var doc = XDocument.Parse(xmlContent);

            // Act
            XNamespace ssdlNs = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
            XNamespace edmNs = "http://schemas.microsoft.com/ado/2009/11/edm";

            var storageEntityType = doc
                .Descendants(ssdlNs + "EntityType")
                .FirstOrDefault(e => e.Attribute("Name")?.Value == "FederalSupplyGroups");

            var conceptualEntityType = doc
                .Descendants(edmNs + "EntityType")
                .FirstOrDefault(e => e.Attribute("Name")?.Value == "FederalSupplyGroup");

            // Assert - All properties should use the same names in both CSDL and SSDL
            var storageCode = storageEntityType
                ?.Elements(ssdlNs + "Property")
                .FirstOrDefault(p => p.Attribute("Name")?.Value == "Code");

            var conceptualCode = conceptualEntityType
                ?.Elements(edmNs + "Property")
                .FirstOrDefault(p => p.Attribute("Name")?.Value == "Code");

            storageCode.Should().NotBeNull();
            conceptualCode.Should().NotBeNull();
            
            storageCode.Attribute("Name")?.Value.Should().Be("Code");
            conceptualCode.Attribute("Name")?.Value.Should().Be("Code");
        }

        #endregion

    }

}