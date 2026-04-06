using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.Tests.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Test to output the generated EDMX for self-referencing relationships to examine the actual XML structure.
    /// </summary>
    [TestClass]
    public class SelfReferencingXmlOutputTest
    {

        [TestMethod]
        public async Task OutputGeneratedEdmxForAnalysis()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
                .Options;

            using var context = new TestDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var converter = new EdmxConverter();
            var result = converter.ConvertToEdmx(context);

            // Output to console for analysis
            Console.WriteLine("=== Generated EDMX Content ===");
            Console.WriteLine(result.EdmxContent);
            Console.WriteLine("=== End EDMX Content ===");

            // Also save to a file for easier analysis
            var outputPath = Path.Combine(Path.GetTempPath(), $"SelfReferencingTest_{DateTime.Now:yyyyMMdd_HHmmss}.edmx");
            await File.WriteAllTextAsync(outputPath, result.EdmxContent);
            
            Console.WriteLine($"\nEDMX saved to: {outputPath}");

            // Verify it contains our Part entity
            result.EdmxContent.Should().Contain("Part", "EDMX should contain Part entity");
        }

    }

}
