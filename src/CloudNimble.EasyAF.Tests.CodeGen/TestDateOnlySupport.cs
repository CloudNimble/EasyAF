using CloudNimble.EasyAF.CodeGen;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{
    [TestClass]
    public class TestDateOnlySupport : CodeGenTestBase
    {
        [TestMethod]
        public void EdmProviderManifestSupportDateOnly()
        {
            // Check that EdmProviderManifest includes DateOnly and TimeOnly
            var manifest = MetadataItem.EdmProviderManifest;
            var storeTypes = manifest.GetStoreTypes();
            
            // Check if DateOnly exists
            var dateOnlyType = storeTypes.FirstOrDefault(t => t.Name == "DateOnly");
            dateOnlyType.Should().NotBeNull("EdmProviderManifest should include DateOnly");
            dateOnlyType.PrimitiveTypeKind.Should().Be(PrimitiveTypeKind.DateOnly);
            
            // Check if TimeOnly exists
            var timeOnlyType = storeTypes.FirstOrDefault(t => t.Name == "TimeOnly");
            timeOnlyType.Should().NotBeNull("EdmProviderManifest should include TimeOnly");
            timeOnlyType.PrimitiveTypeKind.Should().Be(PrimitiveTypeKind.TimeOnly);
        }

        [TestMethod]
        public void SimpleEdmxWithDateOnlyLoading()
        {
            // Very simple EDMX with just DateOnly
            var edmx = """
                <?xml version="1.0" encoding="utf-8"?>
                <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
                    <edmx:Runtime>
                        <edmx:StorageModels>
                            <Schema Namespace="Test.Store" Provider="System.Data.SqlClient" ProviderManifestToken="2012" xmlns="http://schemas.microsoft.com/ado/2009/11/edm/ssdl">
                                <EntityType Name="TestEntity">
                                    <Key>
                                        <PropertyRef Name="Id" />
                                    </Key>
                                    <Property Name="Id" Type="int" Nullable="false" />
                                    <Property Name="TestDate" Type="date" Nullable="false" />
                                </EntityType>
                                <EntityContainer Name="TestContainer">
                                    <EntitySet Name="TestEntities" EntityType="Test.Store.TestEntity" />
                                </EntityContainer>
                            </Schema>
                        </edmx:StorageModels>
                        <edmx:ConceptualModels>
                            <Schema Namespace="Test" Alias="Self" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
                                <EntityType Name="TestEntity">
                                    <Key>
                                        <PropertyRef Name="Id" />
                                    </Key>
                                    <Property Name="Id" Type="Int32" Nullable="false" />
                                    <Property Name="TestDate" Type="DateOnly" Nullable="false" />
                                </EntityType>
                                <EntityContainer Name="TestContext">
                                    <EntitySet Name="TestEntities" EntityType="Self.TestEntity" />
                                </EntityContainer>
                            </Schema>
                        </edmx:ConceptualModels>
                        <edmx:Mappings>
                            <Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2009/11/mapping/cs">
                                <EntityContainerMapping StorageEntityContainer="TestContainer" CdmEntityContainer="TestContext">
                                    <EntitySetMapping Name="TestEntities">
                                        <EntityTypeMapping TypeName="Test.TestEntity">
                                            <MappingFragment StoreEntitySet="TestEntities">
                                                <ScalarProperty Name="Id" ColumnName="Id" />
                                                <ScalarProperty Name="TestDate" ColumnName="TestDate" />
                                            </MappingFragment>
                                        </EntityTypeMapping>
                                    </EntitySetMapping>
                                </EntityContainerMapping>
                            </Mapping>
                        </edmx:Mappings>
                    </edmx:Runtime>
                </edmx:Edmx>
                """;

            try
            {
                var loader = new EdmxLoader();
                loader.Load(edmx);
                
                // Log any errors for debugging
                if (loader.EdmxSchemaErrors.Any())
                {
                    foreach (var error in loader.EdmxSchemaErrors)
                    {
                        Console.WriteLine($"Error: {error.ErrorText} at line {error.Line}, column {error.Column}");
                    }
                }
                
                loader.EdmxSchemaErrors.Should().BeEmpty("EdmxLoader should recognize DateOnly type");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
        }
    }
}