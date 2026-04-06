using CloudNimble.Breakdance.Assemblies;
using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{

    [TestClass]
    public class ApiControllerGeneratorTests : CodeGenTestBase
    {

        #region Private Members

        private const string ApiControllerPath = ProjectPath + @"Baselines\ApiControllers\EasyAFEntitiesApi.Generated.cs";

        #endregion

        #region Properties

        public TestContext TestContext { get; set; }

        public EdmxLoader EdmxLoader { get; private set; }

        #endregion

        #region Test Setup / Teardown

        [TestInitialize]
        public void Initialize()
        {
            //var test = Directory.GetParent(ModelPath);
            //var result = test.Exists;
            var directory = Directory.GetCurrentDirectory();
            EdmxLoader = new EdmxLoader(ModelPath);
            EdmxLoader.Load();
        }

        #endregion

        [TestMethod]
        //[DeploymentItem(ApiControllerPath, "Baselines\\ApiControllers")]
        public void ApiControllerClass()
        {
            using var generator = new ApiControllerGenerator(["EasyAFModel.Core"], EdmxLoader.ModelNamespace, EdmxLoader.EntityContainer, EdmxLoader.IsEFCore);
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
        public void WriteApi(string path)
        {
            using var generator = new ApiControllerGenerator(["EasyAFModel.Core"], EdmxLoader.ModelNamespace, EdmxLoader.EntityContainer, EdmxLoader.IsEFCore);
            generator.Generate();
            generator.WriteFile(GetDirectory(ApiControllerPath));
            File.Exists(Path.Combine(GetDirectory(ApiControllerPath), $"{EdmxLoader.EntityContainer.Name}Api.Generated.cs")).Should().BeTrue();
        }

        [TestMethod]
        public void ApiControllerClass_WithGenericBaseClass()
        {
            // Test with a generic base class specification
            using var generator = new ApiControllerGenerator(
                ["EasyAFModel.Core", "Microsoft.EntityFrameworkCore"],
                EdmxLoader.ModelNamespace,
                EdmxLoader.EntityContainer,
                EdmxLoader.IsEFCore,
                addInheritance: true,
                baseClass: "TestBaseApi<EasyAFEntities>");

            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);

            result.Should().NotBeNullOrWhiteSpace();
            // Verify the class declaration includes the generic base class as specified
            result.Should().Contain("public partial class EasyAFEntitiesApi : TestBaseApi<EasyAFEntities>");
        }

        [TestMethod]
        public void ApiControllerClass_WithoutInheritance()
        {
            // Test without inheritance
            using var generator = new ApiControllerGenerator(
                ["EasyAFModel.Core"],
                EdmxLoader.ModelNamespace,
                EdmxLoader.EntityContainer,
                EdmxLoader.IsEFCore,
                addInheritance: false);

            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);

            result.Should().NotBeNullOrWhiteSpace();
            // Verify the class declaration doesn't include inheritance
            result.Should().Contain("public partial class EasyAFEntitiesApi");
            result.Should().NotContain(" : ");
            // Verify no constructor is generated when not using inheritance
            result.Should().NotContain("public EasyAFEntitiesApi(");
        }

        [TestMethod]
        public void ApiControllerClass_ParseGenericTypeSyntax()
        {
            // This test validates the generic type parsing functionality indirectly
            // by attempting to generate with complex generic types
            using var generator = new ApiControllerGenerator(
                ["EasyAFModel.Core", "System.Collections.Generic"],
                EdmxLoader.ModelNamespace,
                EdmxLoader.EntityContainer,
                EdmxLoader.IsEFCore,
                addInheritance: true,
                baseClass: "ComplexBase<EasyAFEntities, Dictionary<string, object>>");

            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);

            result.Should().NotBeNullOrWhiteSpace();
            // Verify the complex generic base class is preserved
            result.Should().Contain("public partial class EasyAFEntitiesApi : ComplexBase<EasyAFEntities, Dictionary<string, object>>");
        }

    }

}
