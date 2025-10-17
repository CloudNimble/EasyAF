using CloudNimble.EasyAF.CodeGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{
    [TestClass]
    public class DebugDateOnlyTest : CodeGenTestBase
    {
        [TestMethod]
        public void DebugDateOnlyErrors()
        {
            var edmx = """
                <?xml version="1.0" encoding="utf-8"?>
                <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
                    <edmx:Runtime>
                        <edmx:StorageModels>
                            <Schema Namespace="TestModel.Store" Provider="System.Data.SqlClient" ProviderManifestToken="2012" xmlns="http://schemas.microsoft.com/ado/2009/11/edm/ssdl">
                                <EntityType Name="Events">
                                    <Key>
                                        <PropertyRef Name="Id" />
                                    </Key>
                                    <Property Name="Id" Type="int" Nullable="false" />
                                    <Property Name="EventDate" Type="date" Nullable="false" />
                                </EntityType>
                                <EntityContainer Name="TestModelStoreContainer">
                                    <EntitySet Name="Events" EntityType="TestModel.Store.Events" />
                                </EntityContainer>
                            </Schema>
                        </edmx:StorageModels>
                        <edmx:ConceptualModels>
                            <Schema Namespace="TestModel" Alias="Self" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
                                <EntityType Name="Event">
                                    <Key>
                                        <PropertyRef Name="Id" />
                                    </Key>
                                    <Property Name="Id" Type="Int32" Nullable="false" />
                                    <Property Name="EventDate" Type="DateOnly" Nullable="false" />
                                </EntityType>
                                <EntityContainer Name="TestContext">
                                    <EntitySet Name="Events" EntityType="Self.Event" />
                                </EntityContainer>
                            </Schema>
                        </edmx:ConceptualModels>
                        <edmx:Mappings>
                            <Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2009/11/mapping/cs">
                                <EntityContainerMapping StorageEntityContainer="TestModelStoreContainer" CdmEntityContainer="TestContext">
                                    <EntitySetMapping Name="Events">
                                        <EntityTypeMapping TypeName="TestModel.Event">
                                            <MappingFragment StoreEntitySet="Events">
                                                <ScalarProperty Name="Id" ColumnName="Id" />
                                                <ScalarProperty Name="EventDate" ColumnName="EventDate" />
                                            </MappingFragment>
                                        </EntityTypeMapping>
                                    </EntitySetMapping>
                                </EntityContainerMapping>
                            </Mapping>
                        </edmx:Mappings>
                    </edmx:Runtime>
                </edmx:Edmx>
                """;

            var loader = new EdmxLoader();
            loader.Load(edmx);
            
            if (loader.EdmxSchemaErrors.Any())
            {
                Console.WriteLine($"Found {loader.EdmxSchemaErrors.Count} errors:");
                foreach (var error in loader.EdmxSchemaErrors)
                {
                    if (error is not null)
                    {
                        try
                        {
                            var msg = error.ErrorText ?? "No error text";
                            var code = error.ErrorNumber ?? "NO_CODE";
                            Console.WriteLine($"Error {code}: {msg}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error accessing CompilerError: {ex.Message}");
                        }
                    }
                }
                
                Assert.Fail("EDMX loading produced errors");
            }
            
            Console.WriteLine($"Successfully loaded EDMX with {loader.Entities.Count} entities");
            Assert.AreEqual(1, loader.Entities.Count);
            
            var eventEntity = loader.Entities.First();
            Assert.AreEqual("Event", eventEntity.EntityType.Name);
            
            var eventDateProperty = eventEntity.EntityType.Properties.FirstOrDefault(p => p.Name == "EventDate");
            Assert.IsNotNull(eventDateProperty, "EventDate property should exist");
            
            Console.WriteLine($"EventDate property type: {eventDateProperty.TypeName}");
        }
    }
}