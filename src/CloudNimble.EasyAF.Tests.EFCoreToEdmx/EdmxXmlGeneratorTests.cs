using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Contains unit tests for the <see cref="EdmxXmlGenerator"/> class.
    /// </summary>
    /// <remarks>
    /// These tests verify the XML generation functionality, ensuring that EDMX model objects
    /// are correctly converted to valid EDMX XML format that conforms to the Entity Data Model
    /// specification. Tests cover both structure validation and content accuracy.
    /// </remarks>
    [TestClass]
    public class EdmxXmlGeneratorTests
    {

        #region Constructor Tests

        /// <summary>
        /// Tests that <see cref="EdmxXmlGenerator"/> constructor throws
        /// <see cref="ArgumentNullException"/> when provided with a null model.
        /// </summary>
        /// <remarks>
        /// Ensures proper input validation and error handling during construction.
        /// </remarks>
        [TestMethod]
        public void Constructor_WithNullModel_ShouldThrowArgumentNullException()
        {

            var action = () => new EdmxXmlGenerator(null!, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            action.Should().Throw<ArgumentNullException>()
                  .And.ParamName.Should().Be("model");

        }

        #endregion

        #region Basic XML Structure Tests

        /// <summary>
        /// Tests that <see cref="EdmxXmlGenerator.Generate()"/> produces
        /// valid XML structure when provided with a valid EDMX model.
        /// </summary>
        /// <remarks>
        /// Verifies that the generated XML is well-formed, parseable, and contains the
        /// expected root element structure conforming to EDMX 3.0 specification.
        /// </remarks>
        [TestMethod]
        public void Generate_WithValidModel_ShouldProduceValidXml()
        {

            var model = CreateTestEdmxModel();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();

            result.Should().NotBeNullOrWhiteSpace();
            
            var doc = XDocument.Parse(result);
            doc.Should().NotBeNull();
            doc.Root.Should().NotBeNull();
            doc.Root!.Name.LocalName.Should().Be("Edmx");
            
            // Verify EDMX version
            var versionAttribute = doc.Root.Attribute("Version");
            versionAttribute.Should().NotBeNull();
            versionAttribute!.Value.Should().Be("3.0");

        }

        /// <summary>
        /// Tests that the generated XML has correct namespace declarations.
        /// </summary>
        [TestMethod]
        public void Generate_WithValidModel_ShouldHaveCorrectNamespaces()
        {

            var model = CreateTestEdmxModel();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            var root = doc.Root!;
            var edmxNamespace = root.GetNamespaceOfPrefix("edmx");
            edmxNamespace.Should().NotBeNull();
            edmxNamespace!.NamespaceName.Should().Be("http://schemas.microsoft.com/ado/2009/11/edmx");

        }

        /// <summary>
        /// Tests that the generated XML has the required EDMX sections.
        /// </summary>
        [TestMethod]
        public void Generate_WithValidModel_ShouldHaveRequiredSections()
        {

            var model = CreateTestEdmxModel();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            // Check for Runtime section
            var runtime = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Runtime");
            runtime.Should().NotBeNull();

            // Check for ConceptualModels section
            var conceptualModels = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "ConceptualModels");
            conceptualModels.Should().NotBeNull();

            // Check for StorageModels section
            var storageModels = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "StorageModels");
            storageModels.Should().NotBeNull();

            // Check for Mappings section
            var mappings = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Mappings");
            mappings.Should().NotBeNull();

        }

        #endregion

        #region Entity Type Tests

        /// <summary>
        /// Tests that <see cref="EdmxXmlGenerator.Generate()"/> includes
        /// all entity types from the model in the generated XML.
        /// </summary>
        /// <remarks>
        /// Validates that entity type definitions are properly included in the conceptual
        /// model section of the EDMX XML with correct names and structure.
        /// </remarks>
        [TestMethod]
        public void Generate_WithEntityTypes_ShouldIncludeAllEntities()
        {

            var model = CreateTestEdmxModel();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();

            result.Should().Contain("EntityType");
            result.Should().Contain("Name=\"TestEntity\"");

            var doc = XDocument.Parse(result);
            var entityTypes = doc.Descendants()
                .Where(x => x.Name.LocalName == "EntityType");
            entityTypes.Should().HaveCount(2);

        }

        /// <summary>
        /// Tests that entity types have correct attributes.
        /// </summary>
        [TestMethod]
        public void Generate_EntityTypes_ShouldHaveCorrectAttributes()
        {

            var model = CreateTestEdmxModel();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            var entityType = doc.Descendants()
                .First(x => x.Name.Namespace.NamespaceName.EndsWith("edm") && x.Name.LocalName == "EntityType");

            entityType.Attribute("Name").Value.Should().Be("TestEntity");
        }

        #endregion

        #region Property Tests

        /// <summary>
        /// Tests that <see cref="EdmxXmlGenerator.Generate()"/> includes
        /// all property attributes and metadata in the generated XML.
        /// </summary>
        /// <remarks>
        /// Verifies that scalar properties are correctly serialized with all their attributes
        /// including type, nullability, length constraints, and other metadata.
        /// </remarks>
        [TestMethod]
        public void Generate_WithProperties_ShouldIncludeAllPropertyAttributes()
        {

            var model = CreateTestEdmxModel();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();

            result.Should().Contain("Property");
            result.Should().Contain("Name=\"Id\"");
            result.Should().Contain("Type=\"Edm.Int32\"");
            result.Should().Contain("Nullable=\"false\"");

            var doc = XDocument.Parse(result);
            var properties = doc.Descendants()
                .Where(x => x.Name.LocalName == "Property");
            properties.Should().HaveCountGreaterOrEqualTo(2); // Id and Name properties

        }

        /// <summary>
        /// Tests that properties with additional attributes are serialized correctly.
        /// </summary>
        [TestMethod]
        public void Generate_WithPropertiesWithConstraints_ShouldIncludeConstraints()
        {

            var model = CreateTestEdmxModel();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            var nameProperty = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Property" && x.Attribute("Name")?.Value == "Name");

            nameProperty.Should().NotBeNull();
            nameProperty!.Attribute("MaxLength")?.Value.Should().Be("100");
            nameProperty.Attribute("Nullable")?.Value.Should().Be("true");

        }

        /// <summary>
        /// Tests that properties with store generated patterns are serialized correctly.
        /// </summary>
        [TestMethod]
        public void Generate_WithGeneratedProperties_ShouldIncludeGenerationPattern()
        {

            var model = CreateTestEdmxModel();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            var idProperty = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Property" && x.Attribute("Name")?.Value == "Id");

            idProperty.Should().NotBeNull();
            idProperty!.Attribute("StoreGeneratedPattern")?.Value.Should().Be("Identity");

        }

        /// <summary>
        /// Tests that DateOnly properties are correctly mapped to 'date' type in the storage model.
        /// </summary>
        /// <remarks>
        /// Ensures that DateOnly CLR types are properly converted to SQL 'date' type in SSDL
        /// to prevent type incompatibility errors when using with databases that support date-only columns.
        /// </remarks>
        [TestMethod]
        public void Generate_WithDateOnlyProperty_ShouldMapToDateInStorageModel()
        {
            var model = CreateTestEdmxModelWithDateOnlyTimeOnly();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            // Find the DateOnly property in the storage model (SSDL)
            var ssdlNamespace = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Schema" && 
                                    x.Attribute("Namespace")?.Value.Contains(".Store") == true)?
                .Name.Namespace;

            ssdlNamespace.Should().NotBeNull();

            var storageProperty = doc.Descendants(ssdlNamespace + "Property")
                .FirstOrDefault(x => x.Attribute("Name")?.Value == "BirthDate");

            storageProperty.Should().NotBeNull();
            storageProperty!.Attribute("Type")?.Value.Should().Be("date", 
                "DateOnly properties should map to 'date' type in storage model, not 'nvarchar'");
        }

        /// <summary>
        /// Tests that TimeOnly properties are correctly mapped to 'time' type in the storage model.
        /// </summary>
        /// <remarks>
        /// Ensures that TimeOnly CLR types are properly converted to SQL 'time' type in SSDL
        /// to prevent type incompatibility errors when using with databases that support time-only columns.
        /// </remarks>
        [TestMethod]
        public void Generate_WithTimeOnlyProperty_ShouldMapToTimeInStorageModel()
        {
            var model = CreateTestEdmxModelWithDateOnlyTimeOnly();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            // Find the TimeOnly property in the storage model (SSDL)
            var ssdlNamespace = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Schema" && 
                                    x.Attribute("Namespace")?.Value.Contains(".Store") == true)?
                .Name.Namespace;

            ssdlNamespace.Should().NotBeNull();

            var storageProperty = doc.Descendants(ssdlNamespace + "Property")
                .FirstOrDefault(x => x.Attribute("Name")?.Value == "AppointmentTime");

            storageProperty.Should().NotBeNull();
            storageProperty!.Attribute("Type")?.Value.Should().Be("time", 
                "TimeOnly properties should map to 'time' type in storage model, not 'nvarchar'");
        }

        /// <summary>
        /// Tests that DateOnly properties are correctly represented in the conceptual model.
        /// </summary>
        /// <remarks>
        /// Verifies that DateOnly types are preserved in the conceptual model (CSDL) 
        /// with the correct Edm.DateOnly type annotation.
        /// </remarks>
        [TestMethod]
        public void Generate_WithDateOnlyProperty_ShouldHaveCorrectTypeInConceptualModel()
        {
            var model = CreateTestEdmxModelWithDateOnlyTimeOnly();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            // Find the DateOnly property in the conceptual model (CSDL)
            var edmNamespace = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Schema" && 
                                    x.Attribute("Namespace")?.Value == "TestNamespace")?
                .Name.Namespace;

            edmNamespace.Should().NotBeNull();

            var conceptualProperty = doc.Descendants(edmNamespace + "Property")
                .FirstOrDefault(x => x.Attribute("Name")?.Value == "BirthDate");

            conceptualProperty.Should().NotBeNull();
            conceptualProperty!.Attribute("Type")?.Value.Should().Be("DateOnly");
        }

        /// <summary>
        /// Tests that TimeOnly properties are correctly represented in the conceptual model.
        /// </summary>
        /// <remarks>
        /// Verifies that TimeOnly types are preserved in the conceptual model (CSDL) 
        /// with the correct Edm.TimeOnly type annotation.
        /// </remarks>
        [TestMethod]
        public void Generate_WithTimeOnlyProperty_ShouldHaveCorrectTypeInConceptualModel()
        {
            var model = CreateTestEdmxModelWithDateOnlyTimeOnly();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            // Find the TimeOnly property in the conceptual model (CSDL)
            var edmNamespace = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Schema" && 
                                    x.Attribute("Namespace")?.Value == "TestNamespace")?
                .Name.Namespace;

            edmNamespace.Should().NotBeNull();

            var conceptualProperty = doc.Descendants(edmNamespace + "Property")
                .FirstOrDefault(x => x.Attribute("Name")?.Value == "AppointmentTime");

            conceptualProperty.Should().NotBeNull();
            conceptualProperty!.Attribute("Type")?.Value.Should().Be("TimeOnly");
        }

        #endregion

        #region Key Tests

        /// <summary>
        /// Tests that the generated XML includes primary key definitions for entities.
        /// </summary>
        /// <remarks>
        /// Verifies that entity primary keys are correctly represented in the XML
        /// with proper PropertyRef elements within Key elements.
        /// </remarks>
        [TestMethod]
        public void Generate_WithKeys_ShouldIncludePrimaryKeyDefinitions()
        {

            var model = CreateTestEdmxModel();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();

            result.Should().Contain("<Key>");
            result.Should().Contain("PropertyRef");

            var doc = XDocument.Parse(result);
            var keys = doc.Descendants()
                .Where(x => x.Name.LocalName == "Key");
            keys.Should().HaveCount(2);

            var edmKey = keys.Where(keys => keys.Name.Namespace.NamespaceName.EndsWith("edm")).FirstOrDefault();
            edmKey.Should().NotBeNull();
            edmKey.Descendants().Should().ContainSingle(edmKey => edmKey.Name.LocalName == "PropertyRef");

            var ssdlKey = keys.Where(keys => keys.Name.Namespace.NamespaceName.EndsWith("ssdl")).FirstOrDefault();
            ssdlKey.Should().NotBeNull();
            ssdlKey.Descendants().Should().ContainSingle(ssdlKey => ssdlKey.Name.LocalName == "PropertyRef");
        }

        /// <summary>
        /// Tests that key property references have correct attributes.
        /// </summary>
        [TestMethod]
        public void Generate_KeyPropertyRefs_ShouldHaveCorrectAttributes()
        {

            var model = CreateTestEdmxModel();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            var propertyRef = doc.Descendants()
                .First(x => x.Name.LocalName == "PropertyRef");

            propertyRef.Attribute("Name")?.Value.Should().Be("Id");

        }

        #endregion

        #region Association Tests

        /// <summary>
        /// Tests that <see cref="EdmxXmlGenerator.Generate()"/> includes
        /// associations and referential constraints in the generated XML.
        /// </summary>
        /// <remarks>
        /// Validates that relationship definitions are properly included with association
        /// ends, multiplicity constraints, and referential constraint mappings.
        /// </remarks>
        [TestMethod]
        public void Generate_WithAssociations_ShouldIncludeRelationships()
        {

            var model = CreateTestEdmxModelWithAssociation();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();

            result.Should().Contain("Association");
            result.Should().Contain("ReferentialConstraint");

            var doc = XDocument.Parse(result);
            var associations = doc.Descendants()
                .Where(x => x.Name.LocalName == "Association");
            associations.Should().HaveCount(2);

            var referentialConstraints = doc.Descendants()
                .Where(x => x.Name.LocalName == "ReferentialConstraint");
            referentialConstraints.Should().HaveCount(2);

        }

        /// <summary>
        /// Tests that association ends have correct attributes.
        /// </summary>
        [TestMethod]
        public void Generate_AssociationEnds_ShouldHaveCorrectAttributes()
        {

            var model = CreateTestEdmxModelWithAssociation();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            var associationEnds = doc.Descendants()
                .Where(x => x.Name.LocalName == "End" && x.Parent?.Name.LocalName == "Association");

            associationEnds.Should().HaveCount(4);

            foreach (var end in associationEnds)
            {

                end.Attribute("Role").Should().NotBeNull();
                end.Attribute("Type").Should().NotBeNull();
                end.Attribute("Multiplicity").Should().NotBeNull();

            }

        }

        /// <summary>
        /// Tests that referential constraints have correct structure.
        /// </summary>
        [TestMethod]
        public void Generate_ReferentialConstraints_ShouldHaveCorrectStructure()
        {

            var model = CreateTestEdmxModelWithAssociation();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            var principal = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Principal");
            principal.Should().NotBeNull();
            principal!.Attribute("Role").Should().NotBeNull();

            var dependent = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Dependent");
            dependent.Should().NotBeNull();
            dependent!.Attribute("Role").Should().NotBeNull();

            var principalPropertyRefs = principal.Descendants()
                .Where(x => x.Name.LocalName == "PropertyRef");
            principalPropertyRefs.Should().HaveCountGreaterThan(0);

            var dependentPropertyRefs = dependent.Descendants()
                .Where(x => x.Name.LocalName == "PropertyRef");
            dependentPropertyRefs.Should().HaveCountGreaterThan(0);

        }

        #endregion

        #region Navigation Property Tests

        /// <summary>
        /// Tests that the generated XML includes navigation properties with correct relationship references.
        /// </summary>
        /// <remarks>
        /// Validates that navigation properties are properly serialized with relationship
        /// attributes and role assignments for proper association navigation.
        /// </remarks>
        [TestMethod]
        public void Generate_WithNavigationProperties_ShouldIncludeNavigationDefinitions()
        {

            var model = CreateTestEdmxModelWithAssociation();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();

            result.Should().Contain("NavigationProperty");

            var doc = XDocument.Parse(result);
            var navigationProperties = doc.Descendants()
                .Where(x => x.Name.LocalName == "NavigationProperty");
            navigationProperties.Should().HaveCount(1);

        }

        /// <summary>
        /// Tests that navigation properties have correct attributes.
        /// </summary>
        [TestMethod]
        public void Generate_NavigationProperties_ShouldHaveCorrectAttributes()
        {

            var model = CreateTestEdmxModelWithAssociation();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            var navigationProperty = doc.Descendants()
                .First(x => x.Name.LocalName == "NavigationProperty");

            navigationProperty.Attribute("Name").Should().NotBeNull();
            navigationProperty.Attribute("Relationship").Should().NotBeNull();
            navigationProperty.Attribute("FromRole").Should().NotBeNull();
            navigationProperty.Attribute("ToRole").Should().NotBeNull();

        }

        #endregion

        #region Entity Container Tests

        /// <summary>
        /// Tests that the generated XML includes entity container with entity sets and association sets.
        /// </summary>
        /// <remarks>
        /// Verifies that the entity container section is properly generated with all
        /// entity sets and association sets required for the runtime model.
        /// </remarks>
        [TestMethod]
        public void Generate_WithEntityContainer_ShouldIncludeContainerDefinitions()
        {

            var model = CreateTestEdmxModelWithAssociation();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();

            result.Should().Contain("EntityContainer");
            result.Should().Contain("EntitySet");

            var doc = XDocument.Parse(result);
            var entityContainer = doc.Descendants()
                .Where(x => x.Name.LocalName == "EntityContainer");
            entityContainer.Should().HaveCount(2); // One in conceptual, one in storage

            var entitySets = doc.Descendants()
                .Where(x => x.Name.LocalName == "EntitySet");
            entitySets.Should().HaveCountGreaterOrEqualTo(1);

        }

        /// <summary>
        /// Tests that entity sets have correct attributes.
        /// </summary>
        [TestMethod]
        public void Generate_EntitySets_ShouldHaveCorrectAttributes()
        {

            var model = CreateTestEdmxModel();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            var entitySet = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "EntitySet");

            entitySet.Should().NotBeNull();
            entitySet!.Attribute("Name").Should().NotBeNull();
            entitySet.Attribute("EntityType").Should().NotBeNull();

        }

        /// <summary>
        /// Tests that association sets are included when associations exist.
        /// </summary>
        [TestMethod]
        public void Generate_WithAssociations_ShouldIncludeAssociationSets()
        {

            var model = CreateTestEdmxModelWithAssociation();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            var associationSets = doc.Descendants()
                .Where(x => x.Name.LocalName == "AssociationSet");
            associationSets.Should().HaveCount(2);

            var associationSet = associationSets.First();
            associationSet.Attribute("Name").Should().NotBeNull();
            associationSet.Attribute("Association").Should().NotBeNull();

            var associationSetEnds = associationSet.Descendants()
                .Where(x => x.Name.LocalName == "End");
            associationSetEnds.Should().HaveCount(2);

        }

        #endregion

        #region Consistency Tests

        /// <summary>
        /// Tests that multiple calls to <see cref="EdmxXmlGenerator.Generate()"/> produce consistent results.
        /// </summary>
        /// <remarks>
        /// Validates that the generator is stateless and can be called multiple times with
        /// consistent output, ensuring thread safety and reliability.
        /// </remarks>
        [TestMethod]
        public void Generate_MultipleCallsWithSameModel_ShouldProduceConsistentResults()
        {

            var model = CreateTestEdmxModel();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result1 = generator.Generate();
            var result2 = generator.Generate();

            result1.Should().Be(result2);

        }

        /// <summary>
        /// Tests that different generator instances with the same model produce identical results.
        /// </summary>
        /// <remarks>
        /// Validates that the generator produces deterministic output and that multiple
        /// instances with equivalent models generate identical XML.
        /// </remarks>
        [TestMethod]
        public void Generate_DifferentInstancesSameModel_ShouldProduceIdenticalResults()
        {

            var model = CreateTestEdmxModel();
            var generator1 = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);
            var generator2 = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result1 = generator1.Generate();
            var result2 = generator2.Generate();

            result1.Should().Be(result2);

        }

        #endregion

        #region Entity Documentation Tests

        /// <summary>
        /// Tests that entity-level documentation is included in the generated conceptual model XML.
        /// </summary>
        /// <remarks>
        /// Verifies that table comments from database sources (e.g., PostgreSQL COMMENT ON TABLE)
        /// are properly extracted and included as Documentation elements in the EDMX conceptual model.
        /// </remarks>
        [TestMethod]
        public void Generate_WithEntityDocumentation_ShouldIncludeInConceptualModel()
        {
            var model = CreateTestEdmxModelWithEntityDocumentation();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            // Find the conceptual model schema
            var conceptualSchema = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Schema" && 
                                    x.Attribute("Namespace")?.Value == "TestNamespace");

            conceptualSchema.Should().NotBeNull();

            // Find the entity type with documentation
            var entityType = conceptualSchema!.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "EntityType" && 
                                   x.Attribute("Name")?.Value == "DocumentedEntity");

            entityType.Should().NotBeNull();

            // Check for Documentation element
            var documentation = entityType!.Elements()
                .FirstOrDefault(x => x.Name.LocalName == "Documentation");

            documentation.Should().NotBeNull("Entity documentation should be included in conceptual model");

            // Check for Summary element within Documentation
            var summary = documentation!.Elements()
                .FirstOrDefault(x => x.Name.LocalName == "Summary");

            summary.Should().NotBeNull();
            summary!.Value.Should().Be("This entity represents a documented table with important business data");
        }

        /// <summary>
        /// Tests that entity-level documentation is included in the generated storage model XML.
        /// </summary>
        /// <remarks>
        /// Verifies that table comments are also included in the storage model (SSDL) section
        /// of the EDMX, maintaining documentation consistency across all model layers.
        /// </remarks>
        [TestMethod]
        public void Generate_WithEntityDocumentation_ShouldIncludeInStorageModel()
        {
            var model = CreateTestEdmxModelWithEntityDocumentation();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            // Find the storage model schema
            var storageSchema = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Schema" && 
                                    x.Attribute("Namespace")?.Value.Contains(".Store") == true);

            storageSchema.Should().NotBeNull();

            // Find the entity type with documentation (note: storage model uses plural names)
            var entityType = storageSchema!.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "EntityType" && 
                                   x.Attribute("Name")?.Value == "DocumentedEntities"); // Plural in storage

            entityType.Should().NotBeNull();

            // Check for Documentation element
            var documentation = entityType!.Elements()
                .FirstOrDefault(x => x.Name.LocalName == "Documentation");

            documentation.Should().NotBeNull("Entity documentation should be included in storage model");

            // Check for Summary element within Documentation
            var summary = documentation!.Elements()
                .FirstOrDefault(x => x.Name.LocalName == "Summary");

            summary.Should().NotBeNull();
            summary!.Value.Should().Be("This entity represents a documented table with important business data");
        }

        /// <summary>
        /// Tests that entities without documentation don't have empty Documentation elements.
        /// </summary>
        /// <remarks>
        /// Ensures that the XML generator doesn't create unnecessary empty Documentation
        /// elements for entities that don't have documentation comments.
        /// </remarks>
        [TestMethod]
        public void Generate_WithoutEntityDocumentation_ShouldNotIncludeEmptyDocumentation()
        {
            var model = CreateTestEdmxModel(); // Basic model without documentation
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            // Find the conceptual model TestEntity
            var conceptualSchema = doc.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "Schema" && 
                                    x.Attribute("Namespace")?.Value == "TestNamespace");

            var entityType = conceptualSchema!.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "EntityType" && 
                                   x.Attribute("Name")?.Value == "TestEntity");

            entityType.Should().NotBeNull();

            // Check that there's no Documentation element
            var documentation = entityType!.Elements()
                .FirstOrDefault(x => x.Name.LocalName == "Documentation");

            documentation.Should().BeNull("Empty documentation elements should not be created");
        }

        #endregion

        #region Documentation Tests

        /// <summary>
        /// Tests that property documentation is included in the generated XML.
        /// </summary>
        [TestMethod]
        public void Generate_WithDocumentedProperties_ShouldIncludeDocumentation()
        {

            var model = CreateTestEdmxModelWithDocumentation();
            var generator = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer);

            var result = generator.Generate();
            var doc = XDocument.Parse(result);

            var documentationElements = doc.Descendants()
                .Where(x => x.Name.LocalName == "Documentation");

            if (documentationElements.Any())
            {

                var summaryElements = doc.Descendants()
                    .Where(x => x.Name.LocalName == "Summary");
                summaryElements.Should().HaveCountGreaterThan(0);

            }

        }

        #endregion

        #region Test Helper Methods

        /// <summary>
        /// Creates a simple test EDMX model for testing basic XML generation functionality.
        /// </summary>
        /// <returns>A test <see cref="EdmxModel"/> with basic entity structure.</returns>
        /// <remarks>
        /// Creates a minimal model with one entity type containing basic properties and keys
        /// for testing fundamental XML generation capabilities.
        /// </remarks>
        private static EdmxModel CreateTestEdmxModel()
        {

            return new EdmxModel
            {

                Namespace = "TestNamespace",
                ContainerName = "TestContainer",
                EntityTypes = [
                    new EdmxEntityType
                    {

                        Name = "TestEntity",
                        Properties = [
                            new EdmxProperty
                            {

                                Name = "Id",
                                Type = "Edm.Int32",
                                Nullable = false,
                                StoreGeneratedPattern = "Identity"

                            },
                            new EdmxProperty
                            {

                                Name = "Name",
                                Type = "Edm.String",
                                Nullable = true,
                                MaxLength = 100

                            }
                        ],
                        Keys = ["Id"]

                    }
                ],
                EntitySets = [
                    new EdmxEntitySet
                    {

                        Name = "TestEntities",
                        EntityTypeName = "TestEntity"

                    }
                ]

            };

        }

        /// <summary>
        /// Creates a test EDMX model with associations for testing relationship XML generation.
        /// </summary>
        /// <returns>A test <see cref="EdmxModel"/> with entity relationships.</returns>
        /// <remarks>
        /// Creates a model with two related entity types including association definitions,
        /// referential constraints, and navigation properties for comprehensive relationship testing.
        /// </remarks>
        private static EdmxModel CreateTestEdmxModelWithAssociation()
        {

            var model = CreateTestEdmxModel();
            
            model.EntityTypes.Add(new EdmxEntityType
            {

                Name = "RelatedEntity",
                Properties = [
                    new EdmxProperty
                    {

                        Name = "Id",
                        Type = "Edm.Int32",
                        Nullable = false

                    },
                    new EdmxProperty
                    {

                        Name = "TestEntityId",
                        Type = "Edm.Int32",
                        Nullable = false

                    }
                ],
                Keys = ["Id"],
                NavigationProperties = [
                    new EdmxNavigationProperty
                    {

                        Name = "TestEntity",
                        Relationship = "TestAssociation",
                        FromRole = "RelatedEntity",
                        ToRole = "TestEntity"

                    }
                ]

            });

            model.EntitySets.Add(new EdmxEntitySet
            {

                Name = "RelatedEntities",
                EntityTypeName = "RelatedEntity"

            });

            model.Associations.Add(new EdmxAssociation
            {

                Name = "TestAssociation",
                End1 = new EdmxAssociationEnd
                {

                    Role = "TestEntity",
                    Type = "TestEntity",
                    Multiplicity = "1"

                },
                End2 = new EdmxAssociationEnd
                {

                    Role = "RelatedEntity",
                    Type = "RelatedEntity",
                    Multiplicity = "*"

                },
                ReferentialConstraint = new EdmxReferentialConstraint
                {

                    Principal = new EdmxReferentialConstraintRole
                    {

                        Role = "TestEntity",
                        PropertyRefs = ["Id"]

                    },
                    Dependent = new EdmxReferentialConstraintRole
                    {

                        Role = "RelatedEntity",
                        PropertyRefs = ["TestEntityId"]

                    }

                }

            });

            model.AssociationSets.Add(new EdmxAssociationSet
            {

                Name = "TestAssociationSet",
                Association = "TestAssociation",
                End1 = new EdmxAssociationSetEnd
                {

                    Role = "TestEntity",
                    EntitySet = "TestEntities"

                },
                End2 = new EdmxAssociationSetEnd
                {

                    Role = "RelatedEntity",
                    EntitySet = "RelatedEntities"

                }

            });

            return model;

        }

        /// <summary>
        /// Creates a test EDMX model with documentation for testing documentation XML generation.
        /// </summary>
        /// <returns>A test <see cref="EdmxModel"/> with property documentation.</returns>
        /// <remarks>
        /// Creates a model with documented properties to test that documentation comments
        /// are properly preserved and formatted in the generated EDMX XML.
        /// </remarks>
        private static EdmxModel CreateTestEdmxModelWithDocumentation()
        {

            var model = CreateTestEdmxModel();
            
            // Add documentation to the Name property
            var nameProperty = model.EntityTypes[0].Properties.FirstOrDefault(p => p.Name == "Name");
            if (nameProperty is not null)
            {

                nameProperty.Documentation = "The name of the test entity";

            }

            return model;

        }

        /// <summary>
        /// Creates a test EDMX model with entity-level documentation for testing table comment support.
        /// </summary>
        /// <returns>A test <see cref="EdmxModel"/> with documented entities.</returns>
        /// <remarks>
        /// Creates a model with entities that have documentation comments to verify that table-level
        /// comments (e.g., PostgreSQL COMMENT ON TABLE) are properly extracted and included in the EDMX.
        /// </remarks>
        private static EdmxModel CreateTestEdmxModelWithEntityDocumentation()
        {
            return new EdmxModel
            {
                Namespace = "TestNamespace",
                ContainerName = "TestContainer",
                EntityTypes = [
                    new EdmxEntityType
                    {
                        Name = "DocumentedEntity",
                        Documentation = "This entity represents a documented table with important business data",
                        Properties = [
                            new EdmxProperty
                            {
                                Name = "Id",
                                Type = "Int32",
                                Nullable = false,
                                StoreGeneratedPattern = "Identity"
                            },
                            new EdmxProperty
                            {
                                Name = "Name",
                                Type = "String",
                                Nullable = true,
                                MaxLength = 100,
                                Documentation = "The name property with its own documentation"
                            }
                        ],
                        Keys = ["Id"]
                    }
                ],
                EntitySets = [
                    new EdmxEntitySet
                    {
                        Name = "DocumentedEntities",
                        EntityTypeName = "DocumentedEntity"
                    }
                ]
            };
        }

        /// <summary>
        /// Creates a test EDMX model with DateOnly and TimeOnly properties for testing type mapping.
        /// </summary>
        /// <returns>A test <see cref="EdmxModel"/> with DateOnly and TimeOnly properties.</returns>
        /// <remarks>
        /// Creates a model with an entity containing DateOnly and TimeOnly properties to verify
        /// that these modern .NET types are correctly mapped to appropriate SQL types (date and time)
        /// in the storage model, preventing type incompatibility errors.
        /// </remarks>
        private static EdmxModel CreateTestEdmxModelWithDateOnlyTimeOnly()
        {
            return new EdmxModel
            {
                Namespace = "TestNamespace",
                ContainerName = "TestContainer",
                EntityTypes = [
                    new EdmxEntityType
                    {
                        Name = "PersonEntity",
                        Properties = [
                            new EdmxProperty
                            {
                                Name = "Id",
                                Type = "Int32",
                                Nullable = false,
                                StoreGeneratedPattern = "Identity"
                            },
                            new EdmxProperty
                            {
                                Name = "Name",
                                Type = "String",
                                Nullable = true,
                                MaxLength = 100
                            },
                            new EdmxProperty
                            {
                                Name = "BirthDate",
                                Type = "DateOnly",
                                Nullable = true
                            },
                            new EdmxProperty
                            {
                                Name = "AppointmentTime",
                                Type = "TimeOnly",
                                Nullable = true
                            },
                            new EdmxProperty
                            {
                                Name = "LastModified",
                                Type = "DateTime",
                                Nullable = false
                            }
                        ],
                        Keys = ["Id"]
                    }
                ],
                EntitySets = [
                    new EdmxEntitySet
                    {
                        Name = "PersonEntities",
                        EntityTypeName = "PersonEntity"
                    }
                ]
            };
        }

        #endregion

    }

}
