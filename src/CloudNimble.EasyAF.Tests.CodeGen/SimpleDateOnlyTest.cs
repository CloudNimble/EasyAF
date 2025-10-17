using CloudNimble.EasyAF.CodeGen;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{
    [TestClass]
    public class SimpleDateOnlyTest : CodeGenTestBase
    {
        [TestMethod]
        public void DebugDateOnlyLoading()
        {
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

            var loader = new EdmxLoader();
            loader.Load(edmx);
            
            // Print detailed error information
            if (loader.EdmxSchemaErrors.Any())
            {
                var errorMessages = new System.Text.StringBuilder();
                errorMessages.AppendLine($"Found {loader.EdmxSchemaErrors.Count} errors:");
                foreach (var error in loader.EdmxSchemaErrors)
                {
                    if (error is not null)
                    {
                        // Safely access properties
                        var errorCode = error.ErrorNumber ?? "UNKNOWN";
                        var errorText = error.ErrorText ?? "No error text";
                        var fileName = error.FileName ?? "No file";
                        errorMessages.AppendLine($"  Error {errorCode}: {errorText}");
                        errorMessages.AppendLine($"    File: {fileName}, Line: {error.Line}, Column: {error.Column}");
                    }
                    else
                    {
                        errorMessages.AppendLine("  Null error object");
                    }
                }
                
                // Write to a temp file so we can see it
                var tempFile = System.IO.Path.GetTempFileName();
                System.IO.File.WriteAllText(tempFile, errorMessages.ToString());
                Console.WriteLine($"Errors written to: {tempFile}");
                Console.WriteLine(errorMessages.ToString());
                
                // Also fail with a clear message
                Assert.Fail($"EDMX loading failed with {loader.EdmxSchemaErrors.Count} errors. First error: {loader.EdmxSchemaErrors.First()?.ErrorText ?? "Unknown"}");
            }
            else
            {
                Console.WriteLine("No errors found!");
            }
            
            // The test should pass if no errors
            loader.EdmxSchemaErrors.Should().BeEmpty("EDMX should load without errors");
        }
    }
}