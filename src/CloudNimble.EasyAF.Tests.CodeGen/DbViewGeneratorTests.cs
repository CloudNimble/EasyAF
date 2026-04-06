using CloudNimble.Breakdance.Assemblies;
using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{

    [TestClass]
    public class DbViewGeneratorTests : CodeGenTestBase
    {

        #region Private Members

        private const string DbViewsPath = ProjectPath + @"Baselines\DbViews\EasyAFEntities.Views.Generated.cs";

        #endregion

        #region Properties

        public TestContext TestContext { get; set; }

        public EdmxLoader EdmxLoader { get; private set; }

        #endregion

        #region Test Setup / Teardown

        [TestInitialize]
        public  void Initialize()
        {
            EdmxLoader = new EdmxLoader(ModelPath);
            EdmxLoader.Load(true);
        }

        #endregion

        //[TestMethod]
        //[DeploymentItem(DbViewsPath, "Baselines\\DbViews")]
        public void DbViewGenerator_GeneratesFile()
        {
            using var generator = new DbViewGenerator(["EasyAFModel.Core"], EdmxLoader.ModelNamespace, EdmxLoader.EntityContainer, EdmxLoader.Mappings);
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(DbViewsPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        //[TestMethod]
        //[DeploymentItem(DbViewsPath, "Baselines\\DbViews")]
        public void DbViewGenerator_GeneratesHashFile()
        {
            var newHashValue = EdmxLoader.Mappings.ComputeMappingHashValue();
            var oldHashValue = File.ReadAllText(Path.Combine(GetDirectory(DbViewsPath), "MappingHashValue.txt"));
            newHashValue.Should().Be(oldHashValue);
        }

        //[DataRow(ProjectPath)]
        //[TestMethod]
        [BreakdanceManifestGenerator]
        public void WriteDbView(string path)
        {
            using var generator = new DbViewGenerator(["EasyAFModel.Core"], EdmxLoader.ModelNamespace, EdmxLoader.EntityContainer, EdmxLoader.Mappings);
            generator.Generate();
            generator.WriteFile(GetDirectory(DbViewsPath));
            File.Exists(Path.Combine(GetDirectory(DbViewsPath), $"{EdmxLoader.EntityContainer.Name}.Views.Generated.cs")).Should().BeTrue();
        }

        //[DataRow(ProjectPath)]
        //[TestMethod]
        [BreakdanceManifestGenerator]
        public void WriteDbViewHash(string path)
        {
            File.WriteAllText(Path.Combine(GetDirectory(DbViewsPath), "MappingHashValue.txt"), EdmxLoader.Mappings.ComputeMappingHashValue());
            File.Exists(Path.Combine(GetDirectory(DbViewsPath), "MappingHashValue.txt")).Should().BeTrue();
        }

    }

}
