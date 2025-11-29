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
    public class InterceptorGeneratorTests : CodeGenTestBase
    {

        #region Private Members

        private const string InquiryInterceptorPath = ProjectPath + @"Baselines\Interceptors\InquiryInterceptors.Generated.cs";
        private const string ProductInterceptorPath = ProjectPath + @"Baselines\Interceptors\ProductInterceptors.Generated.cs";
        private const string UserInterceptorPath =    ProjectPath + @"Baselines\Interceptors\UserInterceptors.Generated.cs";

        #endregion

        #region Properties

        public TestContext TestContext { get; set; }

        public EdmxLoader EdmxLoader { get; private set; }

        #endregion

        #region Test Setup / Teardown

        [TestInitialize]
        public  void Initialize()
        {
            var directory = Directory.GetCurrentDirectory();
            EdmxLoader = new EdmxLoader(ModelPath);
            EdmxLoader.Load();
        }

        #endregion

        [TestMethod]
        //[DeploymentItem(InquiryInterceptorPath, "Baselines\\Interceptors")]
        public void InquiryInterceptorClass()
        {
            using var generator = new InterceptorGenerator([], "EasyAFModel.Api.Controllers", EdmxLoader.EntityContainer, EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "Inquiry"));
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(InquiryInterceptorPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        [TestMethod]
        //[DeploymentItem(ProductInterceptorPath, "Baselines\\Interceptors")]
        public void ProductInterceptorClass()
        {
            using var generator = new InterceptorGenerator([], "EasyAFModel.Api.Controllers", EdmxLoader.EntityContainer, EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "Product"));
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(ProductInterceptorPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        [TestMethod]
        //[DeploymentItem(UserInterceptorPath, "Baselines\\Interceptors")]
        public void UserInterceptorClass()
        {
            using var generator = new InterceptorGenerator([], "EasyAFModel.Api.Controllers", EdmxLoader.EntityContainer, EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "User"));
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(UserInterceptorPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        //[DataRow(ProjectPath)]
        //[TestMethod]
        [BreakdanceManifestGenerator]
        public void WriteInterceptors(string path)
        {
            var entities = new Dictionary<string, string>
            {
                { "Inquiry", InquiryInterceptorPath },
                { "Product", ProductInterceptorPath },
                { "User", UserInterceptorPath },
            };

            foreach (var entity in entities)
            {
                using var generator = new InterceptorGenerator([], "EasyAFModel.Api.Controllers", EdmxLoader.EntityContainer, EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == entity.Key));
                generator.Generate();
                var newPath = GetDirectory(entity.Value);
                generator.WriteFile(newPath);
                File.Exists(Path.Combine(GetDirectory(entity.Value), $"{entity.Key}Interceptors.Generated.cs")).Should().BeTrue();
            }
        }

    }

}
