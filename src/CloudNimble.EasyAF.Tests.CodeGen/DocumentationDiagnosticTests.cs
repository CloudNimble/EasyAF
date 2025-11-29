using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using CloudNimble.EasyAF.CodeGen.Legacy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.CodeGen
{
    /// <summary>
    /// Diagnostic tests to verify Documentation parsing from EDMX files.
    /// These tests help identify where documentation is lost in the parsing chain.
    /// </summary>
    [TestClass]
    public class DocumentationDiagnosticTests : CodeGenTestBase
    {
        private const string RestierTestEdmxPath = RootPath + @"CloudNimble.EasyAF.Tests.Tools\Baselines\RestierTest.edmx";

        /// <summary>
        /// Test that RestierTest.edmx Documentation elements are parsed and accessible through EdmItemCollection.
        /// </summary>
        [TestMethod]
        public void RestierTest_Documentation_ShouldBePopulated()
        {
            // Arrange
            var loader = new EdmxLoader(RestierTestEdmxPath);

            // Act
            loader.Load(true);

            // Assert - check if there are any errors
            loader.EdmxSchemaErrors.Should().BeEmpty("EDMX should load without errors");
            loader.Entities.Should().NotBeEmpty("EDMX should contain entities");

            // Find User entity (has documentation on entity and properties)
            var userEntity = loader.Entities.FirstOrDefault(e => e.EntityType.Name == "User");
            userEntity.Should().NotBeNull("User entity should exist in the EDMX");

            // Check entity-level documentation
            var entityDoc = MetadataTools.Comment(userEntity.EntityType);
            Console.WriteLine($"User entity Documentation object: {userEntity.EntityType.Documentation}");
            Console.WriteLine($"User entity Documentation.Summary: {userEntity.EntityType.Documentation?.Summary}");
            Console.WriteLine($"User entity documentation via MetadataTools: '{entityDoc}'");
            entityDoc.Should().NotBeEmpty("User entity should have documentation from <Summary>YOUR LUMINARY! YOUR LIBERATOR! CLU!</Summary>");

            // Check property-level documentation (EmailAddress has documentation)
            var emailProperty = userEntity.EntityType.Properties.FirstOrDefault(p => p.Name == "EmailAddress");
            emailProperty.Should().NotBeNull("EmailAddress property should exist");

            var propertyDoc = MetadataTools.Comment(emailProperty);
            Console.WriteLine($"EmailAddress Documentation object: {emailProperty.Documentation}");
            Console.WriteLine($"EmailAddress Documentation.Summary: {emailProperty.Documentation?.Summary}");
            Console.WriteLine($"EmailAddress documentation via MetadataTools: '{propertyDoc}'");
            propertyDoc.Should().NotBeEmpty("EmailAddress property should have documentation from <Summary>You'd better find this you POS.</Summary>");
        }

        /// <summary>
        /// Test inline EDMX with Documentation to verify the parsing infrastructure works.
        /// </summary>
        [TestMethod]
        public void InlineEdmx_WithDocumentation_ShouldBePopulated()
        {
            // Arrange - minimal EDMX with documentation
            var edmx = """
                <?xml version="1.0" encoding="utf-8"?>
                <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
                    <edmx:Runtime>
                        <edmx:StorageModels>
                            <Schema Namespace="TestModel.Store" Provider="System.Data.SqlClient" ProviderManifestToken="2012" xmlns="http://schemas.microsoft.com/ado/2009/11/edm/ssdl">
                                <EntityType Name="Users">
                                    <Key>
                                        <PropertyRef Name="Id" />
                                    </Key>
                                    <Property Name="Id" Type="uniqueidentifier" Nullable="false" />
                                    <Property Name="Name" Type="nvarchar" Nullable="false" />
                                </EntityType>
                                <EntityContainer Name="TestModelStoreContainer">
                                    <EntitySet Name="Users" EntityType="TestModel.Store.Users" />
                                </EntityContainer>
                            </Schema>
                        </edmx:StorageModels>
                        <edmx:ConceptualModels>
                            <Schema Namespace="TestModel" Alias="Self" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
                                <EntityType Name="User">
                                    <Documentation>
                                        <Summary>A user in the system.</Summary>
                                    </Documentation>
                                    <Key>
                                        <PropertyRef Name="Id" />
                                    </Key>
                                    <Property Name="Id" Type="Guid" Nullable="false">
                                        <Documentation>
                                            <Summary>The unique identifier.</Summary>
                                        </Documentation>
                                    </Property>
                                    <Property Name="Name" Type="String" Nullable="false">
                                        <Documentation>
                                            <Summary>The user's display name.</Summary>
                                        </Documentation>
                                    </Property>
                                </EntityType>
                                <EntityContainer Name="TestContext">
                                    <EntitySet Name="Users" EntityType="Self.User" />
                                </EntityContainer>
                            </Schema>
                        </edmx:ConceptualModels>
                        <edmx:Mappings>
                            <Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2009/11/mapping/cs">
                                <EntityContainerMapping StorageEntityContainer="TestModelStoreContainer" CdmEntityContainer="TestContext">
                                    <EntitySetMapping Name="Users">
                                        <EntityTypeMapping TypeName="TestModel.User">
                                            <MappingFragment StoreEntitySet="Users">
                                                <ScalarProperty Name="Id" ColumnName="Id" />
                                                <ScalarProperty Name="Name" ColumnName="Name" />
                                            </MappingFragment>
                                        </EntityTypeMapping>
                                    </EntitySetMapping>
                                </EntityContainerMapping>
                            </Mapping>
                        </edmx:Mappings>
                    </edmx:Runtime>
                </edmx:Edmx>
                """;

            // Act
            var loader = new EdmxLoader();
            loader.Load(edmx);

            // Assert
            loader.EdmxSchemaErrors.Should().BeEmpty();
            loader.Entities.Should().HaveCount(1);

            var userEntity = loader.Entities.First();
            userEntity.EntityType.Name.Should().Be("User");

            // Check entity documentation
            var entityDoc = MetadataTools.Comment(userEntity.EntityType);
            Console.WriteLine($"User entity Documentation object: {userEntity.EntityType.Documentation}");
            Console.WriteLine($"User entity Documentation.Summary: {userEntity.EntityType.Documentation?.Summary}");
            Console.WriteLine($"User entity documentation via MetadataTools: '{entityDoc}'");
            entityDoc.Should().Be("A user in the system.", "Entity should have documentation");

            // Check property documentation
            var idProperty = userEntity.EntityType.Properties.FirstOrDefault(p => p.Name == "Id");
            idProperty.Should().NotBeNull();
            var idDoc = MetadataTools.Comment(idProperty);
            Console.WriteLine($"Id Documentation object: {idProperty.Documentation}");
            Console.WriteLine($"Id Documentation.Summary: {idProperty.Documentation?.Summary}");
            Console.WriteLine($"Id documentation via MetadataTools: '{idDoc}'");
            idDoc.Should().Be("The unique identifier.", "Id property should have documentation");

            var nameProperty = userEntity.EntityType.Properties.FirstOrDefault(p => p.Name == "Name");
            nameProperty.Should().NotBeNull();
            var nameDoc = MetadataTools.Comment(nameProperty);
            Console.WriteLine($"Name Documentation object: {nameProperty.Documentation}");
            Console.WriteLine($"Name Documentation.Summary: {nameProperty.Documentation?.Summary}");
            Console.WriteLine($"Name documentation via MetadataTools: '{nameDoc}'");
            nameDoc.Should().Be("The user's display name.", "Name property should have documentation");
        }

        /// <summary>
        /// Lists all entities and their documentation from RestierTest.edmx for diagnostic purposes.
        /// </summary>
        [TestMethod]
        public void RestierTest_ListAllDocumentation_Diagnostic()
        {
            // Arrange
            var loader = new EdmxLoader(RestierTestEdmxPath);

            // Act
            loader.Load(true);

            // List all documentation
            Console.WriteLine("=== DOCUMENTATION DIAGNOSTIC REPORT ===\n");

            foreach (var entity in loader.Entities.OrderBy(e => e.EntityType.Name))
            {
                var entityDoc = MetadataTools.Comment(entity.EntityType);
                Console.WriteLine($"Entity: {entity.EntityType.Name}");
                Console.WriteLine($"  Documentation object: {entity.EntityType.Documentation}");
                Console.WriteLine($"  Documentation.Summary: {entity.EntityType.Documentation?.Summary ?? "(null)"}");
                Console.WriteLine($"  MetadataTools.Comment: '{entityDoc}'");
                Console.WriteLine();

                foreach (var prop in entity.EntityType.Properties.OrderBy(p => p.Name))
                {
                    var propDoc = MetadataTools.Comment(prop);
                    if (!string.IsNullOrEmpty(propDoc) || prop.Documentation != null)
                    {
                        Console.WriteLine($"  Property: {prop.Name}");
                        Console.WriteLine($"    Documentation object: {prop.Documentation}");
                        Console.WriteLine($"    Documentation.Summary: {prop.Documentation?.Summary ?? "(null)"}");
                        Console.WriteLine($"    MetadataTools.Comment: '{propDoc}'");
                    }
                }
                Console.WriteLine();
            }

            // This test always passes - it's for diagnostic output
        }

        /// <summary>
        /// End-to-end test: generates C# code from RestierTest.edmx and verifies documentation appears in the output.
        /// </summary>
        [TestMethod]
        public void RestierTest_GeneratedCode_ShouldContainDocumentation()
        {
            // Arrange
            var loader = new EdmxLoader(RestierTestEdmxPath);
            loader.Load(true);

            var userEntity = loader.Entities.FirstOrDefault(e => e.EntityType.Name == "User");
            userEntity.Should().NotBeNull("User entity should exist");

            // Act - Generate the entity code
            var generator = new EntityGenerator(new List<string>(), "RstierTest.Data", userEntity);
            generator.Generate();
            var generatedCode = generator.ToString();

            // Assert - Check that documentation appears in the generated code
            Console.WriteLine("=== GENERATED CODE ===");
            Console.WriteLine(generatedCode);
            Console.WriteLine("=== END GENERATED CODE ===");

            // Entity class documentation
            generatedCode.Should().Contain("YOUR LUMINARY! YOUR LIBERATOR! CLU!",
                "User entity documentation should appear in generated code");

            // Property documentation - EmailAddress
            generatedCode.Should().Contain("You'd better find this you POS.",
                "EmailAddress property documentation should appear in generated code");

            // Property documentation - Id
            generatedCode.Should().Contain("The identifier for the record.",
                "Id property documentation should appear in generated code");
        }
    }
}
