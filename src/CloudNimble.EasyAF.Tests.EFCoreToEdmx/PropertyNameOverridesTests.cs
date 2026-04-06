using CloudNimble.EasyAF.EFCoreToEdmx;
using CloudNimble.EasyAF.EFCoreToEdmx.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Tests for PropertyNameOverrides feature in EDMX configuration.
    /// </summary>
    /// <remarks>
    /// These tests verify that PropertyNameOverrides configuration is properly
    /// serialized, deserialized, and applied during database scaffolding to
    /// generate HasColumnName() calls and IgnoreTrackingFields() calls.
    /// </remarks>
    [TestClass]
    public class PropertyNameOverridesTests
    {

        #region Fields

        private EdmxConfigManager _configManager;
        private DatabaseScaffolder _scaffolder;
        private string _tempConfigPath;

        #endregion

        #region Test Setup and Cleanup

        /// <summary>
        /// Initializes test dependencies before each test method execution.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _configManager = new EdmxConfigManager();
            _scaffolder = new DatabaseScaffolder();
            _tempConfigPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.edmx.config");
        }

        /// <summary>
        /// Cleans up test resources after each test method execution.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempConfigPath))
            {
                File.Delete(_tempConfigPath);
            }
        }

        #endregion

        #region Configuration Serialization Tests

        /// <summary>
        /// Tests that PropertyNameOverrides are properly serialized to JSON configuration.
        /// </summary>
        [TestMethod]
        public async Task SaveConfig_WithPropertyNameOverrides_ShouldSerializeCorrectly()
        {
            // Arrange
            var config = new EdmxConfig
            {
                ConnectionStringSource = "appsettings.json:ConnectionStrings:DefaultConnection",
                Provider = "PostgreSQL",
                ContextName = "TestDbContext",
                PropertyNameOverrides = new Dictionary<string, Dictionary<string, string>>
                {
                    ["NationalStockNumbers"] = new Dictionary<string, string>
                    {
                        ["NIIN"] = "Niin",
                        ["FSC"] = "Fsc",
                        ["INC"] = "Inc",
                        ["SOS"] = "Sos"
                    },
                    ["Agents"] = new Dictionary<string, string>
                    {
                        ["SSN"] = "Ssn",
                        ["Person"] = "Persona"
                    }
                }
            };

            // Act
            await _configManager.SaveConfigAsync(config, _tempConfigPath);
            var json = await File.ReadAllTextAsync(_tempConfigPath);

            // Assert
            json.Should().Contain("\"propertyNameOverrides\"");
            json.Should().Contain("\"NationalStockNumbers\"");
            json.Should().Contain("\"NIIN\"");
            json.Should().Contain("\"Niin\"");
            json.Should().Contain("\"Agents\"");
            json.Should().Contain("\"SSN\"");
            json.Should().Contain("\"Ssn\"");
        }

        /// <summary>
        /// Tests that PropertyNameOverrides are properly deserialized from JSON configuration.
        /// </summary>
        [TestMethod]
        public async Task LoadConfig_WithPropertyNameOverrides_ShouldDeserializeCorrectly()
        {
            // Arrange
            var json = @"{
  ""connectionStringSource"": ""appsettings.json:ConnectionStrings:DefaultConnection"",
  ""provider"": ""PostgreSQL"",
  ""contextName"": ""TestDbContext"",
  ""propertyNameOverrides"": {
    ""NationalStockNumbers"": {
      ""NIIN"": ""Niin"",
      ""FSC"": ""Fsc""
    },
    ""Parts"": {
      ""PART_ID"": ""PartId""
    }
  }
}";
            await File.WriteAllTextAsync(_tempConfigPath, json);

            // Act
            var config = await _configManager.LoadConfigAsync(_tempConfigPath);

            // Assert
            config.PropertyNameOverrides.Should().NotBeNull();
            config.PropertyNameOverrides.Should().HaveCount(2);
            config.PropertyNameOverrides["NationalStockNumbers"].Should().HaveCount(2);
            config.PropertyNameOverrides["NationalStockNumbers"]["NIIN"].Should().Be("Niin");
            config.PropertyNameOverrides["NationalStockNumbers"]["FSC"].Should().Be("Fsc");
            config.PropertyNameOverrides["Parts"]["PART_ID"].Should().Be("PartId");
        }

        /// <summary>
        /// Tests that configuration without PropertyNameOverrides still works correctly.
        /// </summary>
        [TestMethod]
        public async Task LoadConfig_WithoutPropertyNameOverrides_ShouldLoadSuccessfully()
        {
            // Arrange
            var json = @"{
  ""connectionStringSource"": ""appsettings.json:ConnectionStrings:DefaultConnection"",
  ""provider"": ""SqlServer"",
  ""contextName"": ""TestDbContext""
}";
            await File.WriteAllTextAsync(_tempConfigPath, json);

            // Act
            var config = await _configManager.LoadConfigAsync(_tempConfigPath);

            // Assert
            config.PropertyNameOverrides.Should().BeNull();
            config.Provider.Should().Be("SqlServer");
            config.ContextName.Should().Be("TestDbContext");
        }

        #endregion

        #region OnModelCreating Enhancement Tests

        /// <summary>
        /// Tests that EnhanceOnModelCreating adds IgnoreTrackingFields to all entities.
        /// </summary>
        [TestMethod]
        public void EnhanceOnModelCreating_ShouldAddIgnoreTrackingFields()
        {
            // Arrange
            var onModelCreating = @"protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).HasMaxLength(100);
    });

    modelBuilder.Entity<Category>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Description);
    });
}";

            // Act
            // We need to use reflection to test the private method
            var scaffolderType = typeof(DatabaseScaffolder);
            var enhanceMethod = scaffolderType.GetMethod("EnhanceOnModelCreating", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            var result = (string)enhanceMethod.Invoke(null, new object[] { onModelCreating, null });

            // Assert
            result.Should().Contain("entity.IgnoreTrackingFields();");
            var ignoreCount = System.Text.RegularExpressions.Regex.Matches(result, @"entity\.IgnoreTrackingFields\(\);").Count;
            ignoreCount.Should().Be(2, "should add IgnoreTrackingFields for both entities");
        }

        /// <summary>
        /// Tests that EnhanceOnModelCreating adds HasColumnName calls based on PropertyNameOverrides.
        /// </summary>
        [TestMethod]
        public void EnhanceOnModelCreating_WithPropertyOverrides_ShouldAddHasColumnName()
        {
            // Arrange
            var onModelCreating = @"protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<NationalStockNumber>(entity =>
    {
        entity.Property(e => e.Niin).HasMaxLength(9);
        entity.Property(e => e.Fsc).HasMaxLength(4);
        entity.Property(e => e.Inc).HasMaxLength(5);
    });
}";

            var propertyOverrides = new Dictionary<string, Dictionary<string, string>>
            {
                ["NationalStockNumber"] = new Dictionary<string, string>
                {
                    ["NIIN"] = "Niin",
                    ["FSC"] = "Fsc",
                    ["INC"] = "Inc"
                }
            };

            // Act
            var scaffolderType = typeof(DatabaseScaffolder);
            var enhanceMethod = scaffolderType.GetMethod("EnhanceOnModelCreating",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            var result = (string)enhanceMethod.Invoke(null, new object[] { onModelCreating, propertyOverrides });

            // Assert
            result.Should().Contain(".HasColumnName(\"NIIN\")");
            result.Should().Contain(".HasColumnName(\"FSC\")");
            result.Should().Contain(".HasColumnName(\"INC\")");
            result.Should().Contain("entity.IgnoreTrackingFields();");
        }

        /// <summary>
        /// Tests that EnhanceOnModelCreating correctly handles self-referencing relationships.
        /// </summary>
        [TestMethod]
        public void EnhanceOnModelCreating_WithSelfReference_ShouldRenameInverseParent()
        {
            // Arrange
            var onModelCreating = @"protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Part>(entity =>
    {
        entity.HasOne(d => d.Parent)
            .WithMany(p => p.InverseParent)
            .HasForeignKey(d => d.ParentId);
    });
}";

            // Act
            var scaffolderType = typeof(DatabaseScaffolder);
            var enhanceMethod = scaffolderType.GetMethod("EnhanceOnModelCreating",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            var result = (string)enhanceMethod.Invoke(null, new object[] { onModelCreating, null });

            // Assert
            result.Should().NotContain("InverseParent");
            result.Should().Contain(".WithMany(p => p.Children)");
            result.Should().Contain("entity.IgnoreTrackingFields();");
        }

        /// <summary>
        /// Tests that EnhanceOnModelCreating preserves PostgreSQL extensions.
        /// </summary>
        [TestMethod]
        public void EnhanceOnModelCreating_WithPostgresExtension_ShouldPreserve()
        {
            // Arrange
            var onModelCreating = @"protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasPostgresExtension(""uuid-ossp"");

    modelBuilder.Entity<Product>(entity =>
    {
        entity.Property(e => e.Id).HasDefaultValueSql(""uuid_generate_v4()"");
    });
}";

            // Act
            var scaffolderType = typeof(DatabaseScaffolder);
            var enhanceMethod = scaffolderType.GetMethod("EnhanceOnModelCreating",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            var result = (string)enhanceMethod.Invoke(null, new object[] { onModelCreating, null });

            // Assert
            result.Should().Contain("modelBuilder.HasPostgresExtension(\"uuid-ossp\");");
            result.Should().Contain("entity.IgnoreTrackingFields();");
            result.Should().Contain(".HasDefaultValueSql(\"uuid_generate_v4()\")");
        }

        #endregion

        #region Integration Scenario Tests

        /// <summary>
        /// Tests a complex scenario with multiple entities and various overrides.
        /// </summary>
        [TestMethod]
        public void EnhanceOnModelCreating_ComplexScenario_ShouldHandleCorrectly()
        {
            // Arrange
            var onModelCreating = @"protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<FederalSupplyClass>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Code).HasMaxLength(4);
    });

    modelBuilder.Entity<NationalStockNumber>(entity =>
    {
        entity.Property(e => e.Niin).HasMaxLength(9);
        entity.Property(e => e.Fsc).HasMaxLength(4);
        entity.Property(e => e.Inc).HasMaxLength(5);
        entity.Property(e => e.Sos).HasMaxLength(20);
    });

    modelBuilder.Entity<Part>(entity =>
    {
        entity.HasOne(d => d.Parent)
            .WithMany(p => p.InverseParent)
            .HasForeignKey(d => d.ParentId);
    });
}";

            var propertyOverrides = new Dictionary<string, Dictionary<string, string>>
            {
                ["NationalStockNumber"] = new Dictionary<string, string>
                {
                    ["NIIN"] = "Niin",
                    ["FSC"] = "Fsc",
                    ["INC"] = "Inc",
                    ["SOS"] = "Sos"
                }
            };

            // Act
            var scaffolderType = typeof(DatabaseScaffolder);
            var enhanceMethod = scaffolderType.GetMethod("EnhanceOnModelCreating",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            var result = (string)enhanceMethod.Invoke(null, new object[] { onModelCreating, propertyOverrides });

            // Assert
            // Check IgnoreTrackingFields for all entities
            var ignoreCount = System.Text.RegularExpressions.Regex.Matches(result, @"entity\.IgnoreTrackingFields\(\);").Count;
            ignoreCount.Should().Be(3, "should add IgnoreTrackingFields for all three entities");

            // Check HasColumnName for NationalStockNumber properties
            result.Should().Contain(".HasColumnName(\"NIIN\")");
            result.Should().Contain(".HasColumnName(\"FSC\")");
            result.Should().Contain(".HasColumnName(\"INC\")");
            result.Should().Contain(".HasColumnName(\"SOS\")");

            // Check that FederalSupplyClass doesn't get HasColumnName (no overrides)
            var lines = result.Split('\n');
            var inFederalSupplyClass = false;
            foreach (var line in lines)
            {
                if (line.Contains("Entity<FederalSupplyClass>"))
                    inFederalSupplyClass = true;
                if (inFederalSupplyClass && line.Contains("});"))
                    inFederalSupplyClass = false;
                if (inFederalSupplyClass)
                    line.Should().NotContain("HasColumnName");
            }

            // Check self-reference fix
            result.Should().NotContain("InverseParent");
            result.Should().Contain("Children");
        }

        #endregion

    }

}