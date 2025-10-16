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
    public class EntityGeneratorTests : CodeGenTestBase
    {

        #region Private Members

        private const string InquiryEntityPath = ProjectPath + @"Baselines\Entities\Inquiry.Generated.cs";
        private const string ProductEntityPath = ProjectPath + @"Baselines\Entities\Product.Generated.cs";
        private const string UserEntityPath =    ProjectPath + @"Baselines\Entities\User.Generated.cs";

        #endregion

        #region Properties

        public TestContext TestContext { get; set; }

        public EdmxLoader EdmxLoader { get; private set; }

        #endregion

        #region Test Setup / Teardown

        [TestInitialize]
        public void Initialize()
        {
            EdmxLoader = new EdmxLoader(ModelPath);
            EdmxLoader.Load();
        }

        #endregion

        [TestMethod]
        //[DeploymentItem(InquiryEntityPath, "Baselines\\Entities")]
        public void InquiryClass()
        {
            using var generator = new EntityGenerator(null, EdmxLoader.ModelNamespace, EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "Inquiry"));
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(InquiryEntityPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        [TestMethod]
        //[DeploymentItem(ProductEntityPath, "Baselines\\Entities")]
        public void ProductClass()
        {
            using var generator = new EntityGenerator(null, EdmxLoader.ModelNamespace, EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "Product"));
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(ProductEntityPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        [TestMethod]
        //[DeploymentItem(UserEntityPath, "Baselines\\Entities")]
        public void UserClass()
        {
            using var generator = new EntityGenerator(null, EdmxLoader.ModelNamespace, EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "User"));
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(UserEntityPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        //[DataRow(ProjectPath)]
        //[TestMethod]
        [BreakdanceManifestGenerator]
        public void WriteEntities(string path)
        {
            var entities = new Dictionary<string, string>
            {
                { "Inquiry", InquiryEntityPath },
                { "Product", ProductEntityPath },
                { "User", UserEntityPath },
                //{ "InquiryStateType", @"Baselines\Entities\InquiryStateType.Generated.cs" },
                //{ "ProductStatusType", @"Baselines\Entities\ProductStatusType.Generated.cs" },
            };

            foreach (var entity in entities)
            {
                using var generator = new EntityGenerator(null, EdmxLoader.ModelNamespace, EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == entity.Key));
                generator.Generate();
                generator.WriteFile(GetDirectory(entity.Value));
                File.Exists(Path.Combine(GetDirectory(entity.Value), $"{entity.Key}.Generated.cs")).Should().BeTrue();
            }
        }

    }

}
