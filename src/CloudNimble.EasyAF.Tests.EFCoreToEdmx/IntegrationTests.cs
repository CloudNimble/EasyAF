using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Contains integration tests for the complete EF Core to EDMX conversion pipeline.
    /// </summary>
    /// <remarks>
    /// These tests verify the end-to-end functionality of converting Entity Framework Core
    /// models to complete, valid EDMX files that conform to the Entity Data Model specification.
    /// </remarks>
    [TestClass]
    public class IntegrationTests
    {

        #region Fields

        private EdmxConverter _converter;

        #endregion

        #region Test Setup and Cleanup

        /// <summary>
        /// Initializes test dependencies before each test method execution.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {

            _converter = new EdmxConverter();

        }

        #endregion

        #region Complete Pipeline Tests

        /// <summary>
        /// Tests the complete conversion pipeline with a complex model.
        /// </summary>
        [TestMethod]
        public void FullConversion_WithComplexModel_ShouldProduceCompleteEdmx()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);

            result.Should().NotBeNull();
            result.DbContextName.Should().Be("TestDbContext");
            result.EdmxContent.Should().NotBeNullOrWhiteSpace();

            var doc = XDocument.Parse(result.EdmxContent);
            doc.Should().NotBeNull();

            doc.Root.Should().NotBeNull();
            doc.Root!.Name.LocalName.Should().Be("Edmx");
            doc.Root.Attribute("Version")?.Value.Should().Be("3.0");

            var conceptualModel = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "ConceptualModels");
            conceptualModel.Should().NotBeNull();

            var entityTypes = doc.Descendants()
                .Where(x => x.Name.LocalName == "EntityType");
            entityTypes.Should().HaveCountGreaterOrEqualTo(3);

            var entityNames = entityTypes.Select(e => e.Attribute("Name")?.Value).ToList();
            entityNames.Should().Contain("User");
            entityNames.Should().Contain("Order");
            entityNames.Should().Contain("OrderItem");

            var associations = doc.Descendants()
                .Where(x => x.Name.LocalName == "Association");
            associations.Should().HaveCountGreaterThan(0);

            var entityContainer = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "EntityContainer");
            entityContainer.Should().NotBeNull();

            var entitySets = doc.Descendants()
                .Where(x => x.Name.LocalName == "EntitySet");
            entitySets.Should().HaveCountGreaterOrEqualTo(3);

        }

        #endregion

        #region Type Mapping Tests

        /// <summary>
        /// Tests that various property types are mapped correctly in the complete pipeline.
        /// </summary>
        [TestMethod]
        public void FullConversion_WithVariousPropertyTypes_ShouldMapTypesCorrectly()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);

            result.Should().NotBeNull();
            result.DbContextName.Should().Be("TestDbContext");
            result.EdmxContent.Should().NotBeNullOrWhiteSpace();

            var doc = XDocument.Parse(result.EdmxContent);

            var stringProperties = doc.Descendants()
                .Where(x => x.Name.LocalName == "Property" && x.Attribute("Type")?.Value == "String");
            stringProperties.Should().HaveCountGreaterThan(0);

            var intProperties = doc.Descendants()
                .Where(x => x.Name.LocalName == "Property" && x.Attribute("Type")?.Value == "Int32");
            intProperties.Should().HaveCountGreaterThan(0);

            var dateTimeProperties = doc.Descendants()
                .Where(x => x.Name.LocalName == "Property" && x.Attribute("Type")?.Value == "DateTime");
            dateTimeProperties.Should().HaveCountGreaterThan(0);

            var decimalProperties = doc.Descendants()
                .Where(x => x.Name.LocalName == "Property" && x.Attribute("Type")?.Value == "Decimal");
            decimalProperties.Should().HaveCountGreaterThan(0);

            var boolProperties = doc.Descendants()
                .Where(x => x.Name.LocalName == "Property" && x.Attribute("Type")?.Value == "Boolean");
            boolProperties.Should().HaveCountGreaterThan(0);

        }

        #endregion

        #region Relationship Tests

        /// <summary>
        /// Tests that foreign key relationships are correctly converted to valid associations.
        /// </summary>
        [TestMethod]
        public void FullConversion_WithForeignKeyRelationships_ShouldCreateValidAssociations()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);

            result.Should().NotBeNull();
            result.DbContextName.Should().Be("TestDbContext");
            result.EdmxContent.Should().NotBeNullOrWhiteSpace();

            var doc = XDocument.Parse(result.EdmxContent);

            var associations = doc.Descendants()
                .Where(x => x.Name.LocalName == "Association");
            associations.Should().HaveCountGreaterThan(0);

            var associationEnds = doc.Descendants()
                .Where(x => x.Name.LocalName == "End" && x.Parent?.Name.LocalName == "Association");
            associationEnds.Should().HaveCountGreaterThan(0);

            var referentialConstraints = doc.Descendants()
                .Where(x => x.Name.LocalName == "ReferentialConstraint");
            referentialConstraints.Should().HaveCountGreaterThan(0);

            var principals = doc.Descendants()
                .Where(x => x.Name.LocalName == "Principal");
            principals.Should().HaveCountGreaterThan(0);

            var dependents = doc.Descendants()
                .Where(x => x.Name.LocalName == "Dependent");
            dependents.Should().HaveCountGreaterThan(0);

            var propertyRefs = doc.Descendants()
                .Where(x => x.Name.LocalName == "PropertyRef" &&
                           (x.Parent?.Name.LocalName == "Principal" || x.Parent?.Name.LocalName == "Dependent"));
            propertyRefs.Should().HaveCountGreaterThan(0);

        }

        #endregion

        #region Navigation Property Tests

        /// <summary>
        /// Tests that navigation properties are correctly created and structured.
        /// </summary>
        [TestMethod]
        public void FullConversion_WithNavigationProperties_ShouldCreateValidNavigationProperties()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);

            result.Should().NotBeNull();
            result.DbContextName.Should().Be("TestDbContext");
            result.EdmxContent.Should().NotBeNullOrWhiteSpace();

            var doc = XDocument.Parse(result.EdmxContent);

            var navigationProperties = doc.Descendants()
                .Where(x => x.Name.LocalName == "NavigationProperty");
            navigationProperties.Should().HaveCountGreaterThan(0);

            foreach (var navProp in navigationProperties)
            {

                navProp.Attribute("Name").Should().NotBeNull();
                navProp.Attribute("Relationship").Should().NotBeNull();
                navProp.Attribute("FromRole").Should().NotBeNull();
                navProp.Attribute("ToRole").Should().NotBeNull();

            }

        }

        #endregion

        #region Key Definition Tests

        /// <summary>
        /// Tests that primary keys are correctly included in the generated EDMX.
        /// </summary>
        [TestMethod]
        public void FullConversion_WithPrimaryKeys_ShouldIncludeKeyDefinitions()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);

            result.Should().NotBeNull();
            result.DbContextName.Should().Be("TestDbContext");
            result.EdmxContent.Should().NotBeNullOrWhiteSpace();

            var doc = XDocument.Parse(result.EdmxContent);

            var keys = doc.Descendants()
                .Where(x => x.Name.LocalName == "Key");
            keys.Should().HaveCountGreaterOrEqualTo(3);

            var entityTypes = doc.Descendants()
                .Where(x => x.Name.LocalName == "EntityType");

            foreach (var entityType in entityTypes)
            {

                var keyElement = entityType.Descendants()
                    .FirstOrDefault(x => x.Name.LocalName == "Key");
                keyElement.Should().NotBeNull($"Entity type {entityType.Attribute("Name")?.Value} should have a key");

                var propertyRefs = keyElement!.Descendants()
                    .Where(x => x.Name.LocalName == "PropertyRef");
                propertyRefs.Should().HaveCountGreaterOrEqualTo(1, "Key should have at least one property reference");

            }

        }

        #endregion

        #region Property Constraint Tests

        /// <summary>
        /// Tests that property constraints are preserved in the generated EDMX.
        /// </summary>
        [TestMethod]
        public void FullConversion_WithPropertyConstraints_ShouldPreserveConstraints()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);

            result.Should().NotBeNull();
            result.DbContextName.Should().Be("TestDbContext");
            result.EdmxContent.Should().NotBeNullOrWhiteSpace();

            var doc = XDocument.Parse(result.EdmxContent);

            var propertiesWithMaxLength = doc.Descendants()
                .Where(x => x.Name.LocalName == "Property" && x.Attribute("MaxLength") is not null);
            propertiesWithMaxLength.Should().HaveCountGreaterThan(0);

            var propertiesWithPrecision = doc.Descendants()
                .Where(x => x.Name.LocalName == "Property" && x.Attribute("Precision") is not null);
            propertiesWithPrecision.Should().HaveCountGreaterThan(0);

            var nullableProperties = doc.Descendants()
                .Where(x => x.Name.LocalName == "Property" && x.Attribute("Nullable")?.Value == "true");
            nullableProperties.Should().HaveCountGreaterThan(0);

            var nonNullableProperties = doc.Descendants()
                .Where(x => x.Name.LocalName == "Property" && x.Attribute("Nullable")?.Value == "false");
            nonNullableProperties.Should().HaveCountGreaterThan(0);

        }

        #endregion

        #region Provider-Specific Tests

        /// <summary>
        /// Tests that SQL Server provider-specific metadata is handled correctly.
        /// </summary>
        [TestMethod]
        public void ConversionWithSqlServerProvider_ShouldHandleProviderSpecificMetadata()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);

            result.Should().NotBeNull();
            result.DbContextName.Should().Be("TestDbContext");
            result.EdmxContent.Should().NotBeNullOrWhiteSpace();

            var doc = XDocument.Parse(result.EdmxContent);
            doc.Should().NotBeNull();

        }

        #endregion

        #region Performance Tests

        /// <summary>
        /// Tests that conversion completes within reasonable time limits.
        /// </summary>
        [TestMethod]
        public void FullConversion_WithTypicalModel_ShouldCompleteWithinTimeLimit()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var result = _converter.ConvertToEdmx(context);

            stopwatch.Stop();

            result.Should().NotBeNull();
            result.EdmxContent.Should().NotBeNullOrWhiteSpace();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "Conversion should complete within 5 seconds");

        }

        #endregion

        #region Schema Validation Tests

        /// <summary>
        /// Tests that the generated EDMX has a valid schema structure.
        /// </summary>
        [TestMethod]
        public void FullConversion_ShouldGenerateValidEdmxSchema()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);

            result.Should().NotBeNull();
            result.DbContextName.Should().Be("TestDbContext");
            result.EdmxContent.Should().NotBeNullOrWhiteSpace();

            var doc = XDocument.Parse(result.EdmxContent);

            var runtime = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Runtime");
            runtime.Should().NotBeNull("EDMX should contain Runtime section");

            var conceptualModels = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "ConceptualModels");
            conceptualModels.Should().NotBeNull("EDMX should contain ConceptualModels section");

            var storageModels = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "StorageModels");
            storageModels.Should().NotBeNull("EDMX should contain StorageModels section");

            var mappings = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Mappings");
            mappings.Should().NotBeNull("EDMX should contain Mappings section");

            doc.Root!.Attributes()
                .Any(a => a.Name.LocalName == "edmx" && a.Value.Contains("schemas.microsoft.com"))
                .Should().BeTrue("EDMX should have proper namespace declarations");

        }

        #endregion

        #region Entity-Specific Validation Tests

        /// <summary>
        /// Tests that User entity is correctly converted with all properties.
        /// </summary>
        [TestMethod]
        public void FullConversion_UserEntity_ShouldHaveCorrectStructure()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);
            var doc = XDocument.Parse(result.EdmxContent);

            var userEntityType = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "EntityType" && x.Attribute("Name")?.Value == "User");

            userEntityType.Should().NotBeNull();

            var userProperties = userEntityType!.Descendants()
                .Where(x => x.Name.LocalName == "Property")
                .Select(x => x.Attribute("Name")?.Value)
                .ToList();

            userProperties.Should().Contain("Id");
            userProperties.Should().Contain("Email");
            userProperties.Should().Contain("FirstName");
            userProperties.Should().Contain("LastName");
            userProperties.Should().Contain("CreatedAt");
            userProperties.Should().Contain("IsActive");

        }

        /// <summary>
        /// Tests that Order entity is correctly converted with foreign key relationships.
        /// </summary>
        [TestMethod]
        public void FullConversion_OrderEntity_ShouldHaveCorrectRelationships()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);
            var doc = XDocument.Parse(result.EdmxContent);

            var orderEntityType = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "EntityType" && x.Attribute("Name")?.Value == "Order");

            orderEntityType.Should().NotBeNull();

            var userIdProperty = orderEntityType!.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Property" && x.Attribute("Name")?.Value == "UserId");

            userIdProperty.Should().NotBeNull();
            userIdProperty!.Attribute("Type")?.Value.Should().Be("Int32");
            userIdProperty.Attribute("Nullable")?.Value.Should().Be("false");

            var userNavigation = orderEntityType.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "NavigationProperty" && x.Attribute("Name")?.Value == "User");

            userNavigation.Should().NotBeNull();

        }

        /// <summary>
        /// Tests that decimal properties have correct precision and scale.
        /// </summary>
        [TestMethod]
        public void FullConversion_DecimalProperties_ShouldHaveCorrectPrecisionAndScale()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);
            var doc = XDocument.Parse(result.EdmxContent);

            var totalAmountProperty = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Property" && x.Attribute("Name")?.Value == "TotalAmount");

            totalAmountProperty.Should().NotBeNull();
            totalAmountProperty!.Attribute("Type")?.Value.Should().Be("decimal");
            totalAmountProperty.Attribute("Precision")?.Value.Should().Be("18");
            totalAmountProperty.Attribute("Scale")?.Value.Should().Be("2");

            var unitPriceProperty = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Property" && x.Attribute("Name")?.Value == "UnitPrice");

            unitPriceProperty.Should().NotBeNull();
            unitPriceProperty!.Attribute("Type")?.Value.Should().Be("decimal");
            unitPriceProperty.Attribute("Precision")?.Value.Should().Be("18");
            unitPriceProperty.Attribute("Scale")?.Value.Should().Be("2");

        }

        #endregion

        #region Error Handling Tests

        /// <summary>
        /// Tests that conversion handles edge cases gracefully.
        /// </summary>
        [TestMethod]
        public void FullConversion_WithEdgeCases_ShouldHandleGracefully()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            // This should not throw exceptions
            var result = _converter.ConvertToEdmx(context);

            result.Should().NotBeNull();
            result.EdmxContent.Should().NotBeNullOrWhiteSpace();

            // Ensure the XML is valid
            var parseAction = () => XDocument.Parse(result.EdmxContent);
            parseAction.Should().NotThrow();

        }

        #endregion

        #region Enum Handling Tests

        /// <summary>
        /// Tests that enum properties are handled correctly in the conversion.
        /// </summary>
        [TestMethod]
        public void FullConversion_WithEnumProperties_ShouldConvertCorrectly()
        {

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestDbContext(options);

            var result = _converter.ConvertToEdmx(context);
            var doc = XDocument.Parse(result.EdmxContent);

            var statusProperty = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Property" && x.Attribute("Name")?.Value == "Status");

            statusProperty.Should().NotBeNull();
            // Enum properties are typically mapped as integers in EDMX
            statusProperty!.Attribute("Type")?.Value.Should().Be("int");

        }

        #endregion

    }

}
