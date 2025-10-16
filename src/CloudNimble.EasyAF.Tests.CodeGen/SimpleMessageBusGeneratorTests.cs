using CloudNimble.Breakdance.Assemblies;
using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Generators.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{

    [TestClass]
    public class SimpleMessageBusGeneratorTests : CodeGenTestBase
    {

        #region Private Members

        private const string DbEntityMessageBasePath = ProjectPath + @"Baselines\SimpleMessageBus\DbEntityMessageBase.Generated.cs";
        private const string UserCreatedPath = ProjectPath + @"Baselines\SimpleMessageBus\UserCreated.Generated.cs";
        private const string UserUpdatedPath = ProjectPath + @"Baselines\SimpleMessageBus\UserUpdated.Generated.cs";
        private const string UserDeletedPath = ProjectPath + @"Baselines\SimpleMessageBus\UserDeleted.Generated.cs";

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
        public void DbEntityMessageBase()
        {
            // For the base class, we use any entity as it's generic
            var userEntity = EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "User");
            using var generator = new SimpleMessageBusGenerator(null, EdmxLoader.ModelNamespace, userEntity, "Base");
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(DbEntityMessageBasePath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        [TestMethod]
        public void UserCreatedMessage()
        {
            var userEntity = EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "User");
            using var generator = new SimpleMessageBusGenerator(null, EdmxLoader.ModelNamespace, userEntity, "Created");
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(UserCreatedPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        [TestMethod]
        public void UserUpdatedMessage()
        {
            var userEntity = EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "User");
            using var generator = new SimpleMessageBusGenerator(null, EdmxLoader.ModelNamespace, userEntity, "Updated");
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(UserUpdatedPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        [TestMethod]
        public void UserDeletedMessage()
        {
            var userEntity = EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "User");
            using var generator = new SimpleMessageBusGenerator(null, EdmxLoader.ModelNamespace, userEntity, "Deleted");
            generator.Generate();
            var result = generator.ToString();
            TestContext.WriteLine(result);
            result.Should().NotBeNullOrWhiteSpace();

            var file = File.ReadAllText(UserDeletedPath);

            // Remove the timestamp from both the generated result and the expected file content
            var sanitizedResult = TimestampRegex().Replace(result, "Date Generated: [TIMESTAMP]");
            var sanitizedFile = TimestampRegex().Replace(file, "Date Generated: [TIMESTAMP]");

            sanitizedResult.Should().Be(sanitizedFile);
        }

        [TestMethod]
        public void CustomNamespace()
        {
            var userEntity = EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "User");
            var customNamespace = $"{EdmxLoader.ModelNamespace}.Messages.Events";
            using var generator = new SimpleMessageBusGenerator(null, customNamespace, userEntity, "Created");
            generator.Generate();
            var result = generator.ToString();
            
            result.Should().Contain($"namespace {customNamespace}");
        }

        [TestMethod]
        public void WriteSimpleMessageBusFiles()
        {
            var userEntity = EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "User");
            var outputDir = Path.Combine(ProjectPath, @"Baselines\SimpleMessageBus");

            // Generate base class
            using (var baseGenerator = new SimpleMessageBusGenerator(null, EdmxLoader.ModelNamespace, userEntity, "Base"))
            {
                baseGenerator.Generate();
                var path = baseGenerator.WriteFile(outputDir);
                File.Exists(path).Should().BeTrue();
            }

            // Generate created message
            using (var createdGenerator = new SimpleMessageBusGenerator(null, EdmxLoader.ModelNamespace, userEntity, "Created"))
            {
                createdGenerator.Generate();
                var path = createdGenerator.WriteFile(outputDir);
                File.Exists(path).Should().BeTrue();
            }

            // Generate updated message
            using (var updatedGenerator = new SimpleMessageBusGenerator(null, EdmxLoader.ModelNamespace, userEntity, "Updated"))
            {
                updatedGenerator.Generate();
                var path = updatedGenerator.WriteFile(outputDir);
                File.Exists(path).Should().BeTrue();
            }

            // Generate deleted message
            using (var deletedGenerator = new SimpleMessageBusGenerator(null, EdmxLoader.ModelNamespace, userEntity, "Deleted"))
            {
                deletedGenerator.Generate();
                var path = deletedGenerator.WriteFile(outputDir);
                File.Exists(path).Should().BeTrue();
            }
        }

        [DataRow(ProjectPath)]
        [TestMethod]
        [BreakdanceManifestGenerator]
        public void WriteAllEntityMessages(string path)
        {
            var outputDir = Path.Combine(ProjectPath, @"Baselines\SimpleMessageBus");

            // Generate base class once
            var firstEntity = EdmxLoader.Entities.First();
            using (var baseGenerator = new SimpleMessageBusGenerator(null, EdmxLoader.ModelNamespace, firstEntity, "Base"))
            {
                baseGenerator.Generate();
                baseGenerator.WriteFile(outputDir);
            }

            // Generate messages for each entity
            foreach (var entity in EdmxLoader.Entities)
            {
                var messageTypes = new[] { "Created", "Updated", "Deleted" };
                
                foreach (var messageType in messageTypes)
                {
                    using var generator = new SimpleMessageBusGenerator(null, EdmxLoader.ModelNamespace, entity, messageType);
                    generator.Generate();
                    generator.WriteFile(outputDir);
                }
            }
        }

    }

}
