using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    [TestClass]
    public class EasyAFLongDescriptionTests
    {

        [TestMethod]
        public void Apply_ShouldStampTableAndColumnLongDescriptionFromInMemoryCatalog()
        {
            var model = CreatePostsModel();
            var catalog = new Dictionary<(string Table, string Column), string>
            {
                [("posts", null)] = "ClipId is optional. When IsFullEpisode is true, metrics go to EpisodeMetrics.",
                [("posts", "isfullepisode")] = "When true, metrics go to EpisodeMetrics, not PostMetrics."
            };

            EasyAFLongDescription.Apply(model, catalog);

            var entity = model.EntityTypes[0];
            entity.LongDescription.Should().Be("ClipId is optional. When IsFullEpisode is true, metrics go to EpisodeMetrics.");
            entity.Properties.First(p => p.Name == "IsFullEpisode").LongDescription
                .Should().Be("When true, metrics go to EpisodeMetrics, not PostMetrics.");
            entity.Documentation.Should().Be("A single platform destination for content.");
        }

        [TestMethod]
        public void Apply_FromRows_ShouldBuildCatalogAndLookupByTable()
        {
            var model = CreatePostsModel();

            EasyAFLongDescription.Apply(model,
            [
                ("Posts", null, "ClipId is optional. When IsFullEpisode is true, metrics go to EpisodeMetrics."),
                ("Posts", "IsFullEpisode", "When true, metrics go to EpisodeMetrics, not PostMetrics.")
            ]);

            var entity = model.EntityTypes[0];
            entity.LongDescription.Should().Be("ClipId is optional. When IsFullEpisode is true, metrics go to EpisodeMetrics.");
            entity.Properties.First(p => p.Name == "IsFullEpisode").LongDescription
                .Should().Be("When true, metrics go to EpisodeMetrics, not PostMetrics.");
        }

        [TestMethod]
        public void PreserveExisting_ShouldFillEmptyLongDescriptionAndLeaveCatalogValues()
        {
            var model = CreatePostsModel();
            model.EntityTypes[0].LongDescription = "From catalog.";

            var existing = """
                <?xml version="1.0" encoding="utf-8"?>
                <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
                  <edmx:Runtime>
                    <edmx:ConceptualModels>
                      <Schema Namespace="Test" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
                        <EntityType Name="Post">
                          <Documentation>
                            <Summary>Hand-authored summary.</Summary>
                            <LongDescription>Hand-authored long description.</LongDescription>
                          </Documentation>
                          <Property Name="IsFullEpisode" Type="Boolean" Nullable="false">
                            <Documentation>
                              <LongDescription>Hand-authored column remarks.</LongDescription>
                            </Documentation>
                          </Property>
                        </EntityType>
                      </Schema>
                    </edmx:ConceptualModels>
                  </edmx:Runtime>
                </edmx:Edmx>
                """;

            EasyAFLongDescription.PreserveExisting(model, existing);

            var entity = model.EntityTypes[0];
            entity.Documentation.Should().Be("A single platform destination for content.", "catalog Summary is already set");
            entity.LongDescription.Should().Be("From catalog.", "catalog LongDescription is already set");
            entity.Properties.First(p => p.Name == "IsFullEpisode").LongDescription
                .Should().Be("Hand-authored column remarks.", "empty model field is filled from existing CSDL");
        }

        [TestMethod]
        public void PreserveExisting_WithoutCatalogProperty_ShouldKeepHandAuthoredLongDescription()
        {
            var model = CreatePostsModel();

            var existing = """
                <?xml version="1.0" encoding="utf-8"?>
                <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
                  <edmx:Runtime>
                    <edmx:ConceptualModels>
                      <Schema Namespace="Test" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
                        <EntityType Name="Post">
                          <Documentation>
                            <LongDescription>Hand-authored long description.</LongDescription>
                          </Documentation>
                        </EntityType>
                      </Schema>
                    </edmx:ConceptualModels>
                  </edmx:Runtime>
                </edmx:Edmx>
                """;

            EasyAFLongDescription.PreserveExisting(model, existing);

            model.EntityTypes[0].LongDescription.Should().Be("Hand-authored long description.");
        }

        [TestMethod]
        public void Generate_AfterPreserveExisting_ShouldEmitHandAuthoredLongDescription()
        {
            var model = CreatePostsModel();
            var existing = """
                <?xml version="1.0" encoding="utf-8"?>
                <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
                  <edmx:Runtime>
                    <edmx:ConceptualModels>
                      <Schema Namespace="Test" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
                        <EntityType Name="Post">
                          <Documentation>
                            <LongDescription>Hand-authored long description.</LongDescription>
                          </Documentation>
                        </EntityType>
                      </Schema>
                    </edmx:ConceptualModels>
                  </edmx:Runtime>
                </edmx:Edmx>
                """;

            EasyAFLongDescription.PreserveExisting(model, existing);
            var xml = new EdmxXmlGenerator(model, CloudNimble.EasyAF.EFCoreToEdmx.DatabaseProviderType.SqlServer).Generate();
            var doc = XDocument.Parse(xml);

            var entity = doc.Descendants()
                .First(x => x.Name.LocalName == "EntityType" && x.Attribute("Name")?.Value == "Post");
            var documentation = entity.Elements().First(x => x.Name.LocalName == "Documentation");
            documentation.Elements().First(x => x.Name.LocalName == "Summary").Value
                .Should().Be("A single platform destination for content.");
            documentation.Elements().First(x => x.Name.LocalName == "LongDescription").Value
                .Should().Be("Hand-authored long description.");
        }

        private static EdmxModel CreatePostsModel()
        {
            return new EdmxModel
            {
                Namespace = "Test",
                ContainerName = "TestContext",
                EntityTypes =
                [
                    new EdmxEntityType
                    {
                        Name = "Post",
                        Documentation = "A single platform destination for content.",
                        Properties =
                        [
                            new EdmxProperty { Name = "Id", Type = "Guid", Nullable = false },
                            new EdmxProperty { Name = "IsFullEpisode", Type = "Boolean", Nullable = false, StoreColumnName = "IsFullEpisode" }
                        ],
                        Keys = ["Id"]
                    }
                ],
                EntitySets =
                [
                    new EdmxEntitySet { Name = "Posts", EntityTypeName = "Post" }
                ]
            };
        }

    }

}
