using CloudNimble.Breakdance.Assemblies;
using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{

    [TestClass]
    public class ManagerGeneratorTests : CodeGenTestBase
    {

        #region Private Members

        private const string InquiryManagerPath = ProjectPath + @"Baselines\Managers\InquiryManager.Generated.cs";
        private const string ProductManagerPath = ProjectPath + @"Baselines\Managers\ProductManager.Generated.cs";
        private const string UserManagerPath =    ProjectPath + @"Baselines\Managers\UserManager.Generated.cs";

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
        //[DeploymentItem(InquiryManagerPath, "Baselines\\Managers")]
        public void InquiryManagerClass()
        {
            using var generator = new ManagerGenerator([], "EasyAFModel.Managers", EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "Inquiry"), EdmxLoader.EntityContainer.Name);
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(InquiryManagerPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        [TestMethod]
        //[DeploymentItem(ProductManagerPath, "Baselines\\Managers")]
        public void ProductManagerClass()
        {
            using var generator = new ManagerGenerator([], "EasyAFModel.Managers", EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "Product"), EdmxLoader.EntityContainer.Name);
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(ProductManagerPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        [TestMethod]
        //[DeploymentItem(UserManagerPath, "Baselines\\Managers")]
        public void UserManagerClass()
        {
            using var generator = new ManagerGenerator([], "EasyAFModel.Managers", EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "User"), EdmxLoader.EntityContainer.Name);
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(UserManagerPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        //[DataRow(ProjectPath)]
        //[TestMethod]
        [BreakdanceManifestGenerator]
        public void WriteManagers(string path)
        {
            var entities = new Dictionary<string, string>
            {
                { "Inquiry", InquiryManagerPath },
                { "Product", ProductManagerPath },
                { "User", UserManagerPath },
            };

            foreach (var entity in entities)
            {
                using var generator = new ManagerGenerator([], "EasyAFModel.Managers", EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == entity.Key), EdmxLoader.EntityContainer.Name);
                generator.Generate();
                generator.WriteFile(GetDirectory(entity.Value));
                File.Exists(Path.Combine(GetDirectory(entity.Value), $"{entity.Key}Manager.Generated.cs")).Should().BeTrue();
            }
        }

    }

}
