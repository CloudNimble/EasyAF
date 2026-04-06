using CloudNimble.Breakdance.Assemblies;
using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{

    [TestClass]
    public class AdminApiControllerGeneratorTests : CodeGenTestBase
    {

        #region Private Members

        private const string ApiControllerPath = ProjectPath + @"Baselines\ApiControllers\EasyAFEntitiesAdminApi.Generated.cs";

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
        //[DeploymentItem(ApiControllerPath, "Baselines\\ApiControllers")]
        public void ApiControllerClass()
        {
            using var generator = new AdminApiControllerGenerator(["EasyAFModel.Core"], EdmxLoader.ModelNamespace, EdmxLoader, EdmxLoader.IsEFCore);
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(ApiControllerPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        //[DataRow(ProjectPath)]
        //[TestMethod]
        [BreakdanceManifestGenerator]
        public void WriteAdminApi(string path)
        {
            using var generator = new AdminApiControllerGenerator(["EasyAFModel.Core"], EdmxLoader.ModelNamespace, EdmxLoader, EdmxLoader.IsEFCore);
            generator.Generate();
            generator.WriteFile(GetDirectory(ApiControllerPath));
            File.Exists(Path.Combine(GetDirectory(ApiControllerPath), $"{EdmxLoader.EntityContainer.Name}AdminApi.Generated.cs")).Should().BeTrue();
        }

    }

}
