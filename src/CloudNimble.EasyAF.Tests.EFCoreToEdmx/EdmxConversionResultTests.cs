using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Contains unit tests for the <see cref="EdmxConversionResult"/> class.
    /// </summary>
    /// <remarks>
    /// These tests verify the functionality of the conversion result model including
    /// property handling, file operations, and edge case scenarios.
    /// </remarks>
    [TestClass]
    public class EdmxConversionResultTests
    {

        #region Fields

        private string _tempDirectory;

        #endregion

        #region Test Setup and Cleanup

        /// <summary>
        /// Initializes test dependencies before each test method execution.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {

            _tempDirectory = Path.Combine(Path.GetTempPath(), "EdmxConversionResultTests_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(_tempDirectory);

        }

        /// <summary>
        /// Cleans up test resources after each test method execution.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {

            if (Directory.Exists(_tempDirectory))
            {

                Directory.Delete(_tempDirectory, true);

            }

        }

        #endregion

        #region Constructor Tests

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult"/> constructor sets properties correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidParameters_ShouldSetProperties()
        {

            var dbContextName = "TestDbContext";
            var edmxContent = "<edmx>test content</edmx>";

            var result = new EdmxConversionResult(dbContextName, edmxContent);

            result.DbContextName.Should().Be(dbContextName);
            result.EdmxContent.Should().Be(edmxContent);

        }

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult"/> constructor handles empty strings.
        /// </summary>
        [TestMethod]
        public void Constructor_WithEmptyStrings_ShouldSetEmptyProperties()
        {

            var result = new EdmxConversionResult("", "");

            result.DbContextName.Should().Be("");
            result.EdmxContent.Should().Be("");

        }

        #endregion

        #region GetFileName Tests

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult.GetFileName"/> returns correct file name.
        /// </summary>
        [TestMethod]
        public void GetFileName_WithValidContextName_ShouldReturnCorrectFileName()
        {

            var result = new EdmxConversionResult("MyDbContext", "<edmx></edmx>");

            var fileName = result.GetFileName();

            fileName.Should().Be("MyDbContext.edmx");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult.GetFileName"/> handles empty context name.
        /// </summary>
        [TestMethod]
        public void GetFileName_WithEmptyContextName_ShouldReturnEdmxExtension()
        {

            var result = new EdmxConversionResult("", "<edmx></edmx>");

            var fileName = result.GetFileName();

            fileName.Should().Be(".edmx");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult.GetFileName"/> handles special characters.
        /// </summary>
        [TestMethod]
        public void GetFileName_WithSpecialCharacters_ShouldPreserveCharacters()
        {

            var result = new EdmxConversionResult("My-Db_Context123", "<edmx></edmx>");

            var fileName = result.GetFileName();

            fileName.Should().Be("My-Db_Context123.edmx");

        }

        #endregion

        #region WriteToFolder Tests

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult.WriteToFolder"/> creates file correctly.
        /// </summary>
        [TestMethod]
        public async Task WriteToFolder_WithValidPath_ShouldCreateFile()
        {

            var edmxContent = """
                <?xml version="1.0" encoding="utf-8"?>
                <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
                  <edmx:Runtime>
                    <edmx:ConceptualModels>
                      <Schema Namespace="TestNamespace" />
                    </edmx:ConceptualModels>
                  </edmx:Runtime>
                </edmx:Edmx>
                """;

            var result = new EdmxConversionResult("TestDbContext", edmxContent);

            await result.WriteToFolder(_tempDirectory);

            var expectedFilePath = Path.Combine(_tempDirectory, "TestDbContext.edmx");
            File.Exists(expectedFilePath).Should().BeTrue();

            var writtenContent = await File.ReadAllTextAsync(expectedFilePath);
            writtenContent.Should().Be(edmxContent);

        }

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult.WriteToFolder"/> creates directory if it doesn't exist.
        /// </summary>
        [TestMethod]
        public async Task WriteToFolder_WithNonExistentDirectory_ShouldCreateDirectory()
        {

            var nonExistentDir = Path.Combine(_tempDirectory, "subfolder", "nested");
            var result = new EdmxConversionResult("TestDbContext", "<edmx></edmx>");

            await result.WriteToFolder(nonExistentDir);

            Directory.Exists(nonExistentDir).Should().BeTrue();

            var expectedFilePath = Path.Combine(nonExistentDir, "TestDbContext.edmx");
            File.Exists(expectedFilePath).Should().BeTrue();

        }

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult.WriteToFolder"/> overwrites existing files.
        /// </summary>
        [TestMethod]
        public async Task WriteToFolder_WithExistingFile_ShouldOverwriteFile()
        {

            var result = new EdmxConversionResult("TestDbContext", "<edmx>new content</edmx>");
            var filePath = Path.Combine(_tempDirectory, "TestDbContext.edmx");

            // Create existing file with different content
            await File.WriteAllTextAsync(filePath, "<edmx>old content</edmx>");

            await result.WriteToFolder(_tempDirectory);

            var writtenContent = await File.ReadAllTextAsync(filePath);
            writtenContent.Should().Be("<edmx>new content</edmx>");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult.WriteToFolder"/> throws exception for null path.
        /// </summary>
        [TestMethod]
        public async Task WriteToFolder_WithNullPath_ShouldThrowArgumentException()
        {

            var result = new EdmxConversionResult("TestDbContext", "<edmx></edmx>");

            var action = async () => await result.WriteToFolder(null!);
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("folderPath");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult.WriteToFolder"/> throws exception for empty path.
        /// </summary>
        [TestMethod]
        public async Task WriteToFolder_WithEmptyPath_ShouldThrowArgumentException()
        {

            var result = new EdmxConversionResult("TestDbContext", "<edmx></edmx>");

            var action = async () => await result.WriteToFolder("");
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("folderPath");

        }

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult.WriteToFolder"/> throws exception for whitespace path.
        /// </summary>
        [TestMethod]
        public async Task WriteToFolder_WithWhitespacePath_ShouldThrowArgumentException()
        {

            var result = new EdmxConversionResult("TestDbContext", "<edmx></edmx>");

            var action = async () => await result.WriteToFolder("   ");
            await action.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("folderPath");

        }

        #endregion

        #region Edge Case Tests

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult"/> handles large EDMX content correctly.
        /// </summary>
        [TestMethod]
        public async Task WriteToFolder_WithLargeContent_ShouldHandleCorrectly()
        {

            // Create large EDMX content (approximately 1MB)
            var largeContent = new string('x', 1024 * 1024);
            var edmxContent = $"<edmx>{largeContent}</edmx>";

            var result = new EdmxConversionResult("LargeDbContext", edmxContent);

            await result.WriteToFolder(_tempDirectory);

            var filePath = Path.Combine(_tempDirectory, "LargeDbContext.edmx");
            File.Exists(filePath).Should().BeTrue();

            var fileInfo = new FileInfo(filePath);
            fileInfo.Length.Should().BeGreaterThan(1024 * 1024);

        }

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult"/> handles special XML characters correctly.
        /// </summary>
        [TestMethod]
        public async Task WriteToFolder_WithSpecialXmlCharacters_ShouldPreserveCharacters()
        {

            var edmxContent = """
                <edmx>
                  <test attribute="value &lt; &gt; &amp; &quot; &apos;">
                    Content with special characters: &lt;&gt;&amp;"'
                  </test>
                </edmx>
                """;

            var result = new EdmxConversionResult("SpecialCharsDbContext", edmxContent);

            await result.WriteToFolder(_tempDirectory);

            var filePath = Path.Combine(_tempDirectory, "SpecialCharsDbContext.edmx");
            var writtenContent = await File.ReadAllTextAsync(filePath);

            writtenContent.Should().Be(edmxContent);

        }

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult"/> handles Unicode content correctly.
        /// </summary>
        [TestMethod]
        public async Task WriteToFolder_WithUnicodeContent_ShouldPreserveUnicode()
        {

            var edmxContent = """
                <edmx>
                  <entity name="用户">
                    <property name="名称">测试</property>
                    <property name="描述">Description with émojis: 🚀✨🎉</property>
                  </entity>
                </edmx>
                """;

            var result = new EdmxConversionResult("UnicodeDbContext", edmxContent);

            await result.WriteToFolder(_tempDirectory);

            var filePath = Path.Combine(_tempDirectory, "UnicodeDbContext.edmx");
            var writtenContent = await File.ReadAllTextAsync(filePath);

            writtenContent.Should().Be(edmxContent);

        }

        /// <summary>
        /// Tests that <see cref="EdmxConversionResult"/> handles empty EDMX content.
        /// </summary>
        [TestMethod]
        public async Task WriteToFolder_WithEmptyContent_ShouldCreateEmptyFile()
        {

            var result = new EdmxConversionResult("EmptyDbContext", "");

            await result.WriteToFolder(_tempDirectory);

            var filePath = Path.Combine(_tempDirectory, "EmptyDbContext.edmx");
            File.Exists(filePath).Should().BeTrue();

            var fileInfo = new FileInfo(filePath);
            fileInfo.Length.Should().Be(0);

        }

        #endregion

        #region Property Tests

        /// <summary>
        /// Tests that properties can be modified after construction.
        /// </summary>
        [TestMethod]
        public void Properties_AfterConstruction_ShouldBeModifiable()
        {

            var result = new EdmxConversionResult("InitialContext", "<initial></initial>");

            result.DbContextName = "ModifiedContext";
            result.EdmxContent = "<modified></modified>";

            result.DbContextName.Should().Be("ModifiedContext");
            result.EdmxContent.Should().Be("<modified></modified>");

        }

        /// <summary>
        /// Tests that properties handle null values correctly.
        /// </summary>
        [TestMethod]
        public void Properties_WithNullValues_ShouldHandleCorrectly()
        {

            var result = new EdmxConversionResult("TestContext", "<test></test>");

            result.DbContextName = null!;
            result.EdmxContent = null!;

            result.DbContextName.Should().BeNull();
            result.EdmxContent.Should().BeNull();

        }

        #endregion

    }

}
