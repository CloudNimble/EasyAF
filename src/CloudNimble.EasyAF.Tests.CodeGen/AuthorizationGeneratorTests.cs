using CloudNimble.Breakdance.Assemblies;
using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{

    [TestClass]
    public class AuthorizationGeneratorTests : CodeGenTestBase
    {

        #region Private Members

        private const string AuthorizationPath = ProjectPath + @"Baselines\Authorization\EasyAFEntitiesAuthorizationConfig.Generated.cs";

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
        //[DeploymentItem(AuthorizationPath, "Baselines\\Authorization")]
        public void AuthorizationClass()
        {
            using var generator = new AuthorizationGenerator(["EasyAFModel.Core"], "EasyAFTests.Api", EdmxLoader.EntityContainer);
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(AuthorizationPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        //[DataRow(ProjectPath)]
        //[TestMethod]
        [BreakdanceManifestGenerator]
        public void WriteAuthorizationClass(string path)
        {
            using var generator = new AuthorizationGenerator(["EasyAFModel.Core"], "EasyAFTests.Api", EdmxLoader.EntityContainer);
            generator.Generate();
            generator.WriteFile(GetDirectory(AuthorizationPath));
            File.Exists(Path.Combine(GetDirectory(AuthorizationPath), $"{EdmxLoader.EntityContainer.Name}AuthorizationConfig.Generated.cs")).Should().BeTrue();
        }

    }

}
