using CloudNimble.Breakdance.Assemblies;
using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{

    [TestClass]
    public class DbContextGeneratorTests : CodeGenTestBase
    {

        #region Private Members

        private const string DbContextPath = ProjectPath + @"Baselines\DbContexts\EasyAFEntities.Generated.cs";

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
            EdmxLoader.Load();
        }

        #endregion

        [TestMethod]
        //[DeploymentItem(DbContextPath, "Baselines\\DbContexts")]
        public void DbContextClass()
        {
            using var generator = new DbContextPartialGenerator(["EasyAFModel.Core"], EdmxLoader.ModelNamespace, EdmxLoader.EntityContainer, EdmxLoader.OnModelCreatingMethod, EdmxLoader.FilePath);
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(DbContextPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        //[DataRow(ProjectPath)]
        //[TestMethod]
        [BreakdanceManifestGenerator]
        public void WriteDbContext(string path)
        {
            using var generator = new DbContextPartialGenerator(["EasyAFModel.Core"], EdmxLoader.ModelNamespace, EdmxLoader.EntityContainer, EdmxLoader.OnModelCreatingMethod, EdmxLoader.FilePath);
            generator.Generate();
            generator.WriteFile(GetDirectory(DbContextPath));
            File.Exists(Path.Combine(GetDirectory(DbContextPath), $"{EdmxLoader.EntityContainer.Name}.Generated.cs")).Should().BeTrue();
        }


    }

}
