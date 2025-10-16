using CloudNimble.EasyAF.EFCoreToEdmx;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CloudNimble.EasyAF.Tests.EFCoreToEdmx
{

    /// <summary>
    /// Tests for OnModelCreating formatting enhancements.
    /// </summary>
    [TestClass]
    public class OnModelCreatingFormattingTests
    {

        #region Formatting Tests

        /// <summary>
        /// Tests that semicolons are preserved in the enhanced OnModelCreating.
        /// </summary>
        [TestMethod]
        public void EnhanceOnModelCreating_ShouldPreserveSemicolons()
        {
            // Arrange
            var onModelCreating = @"protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasPostgresExtension(""uuid-ossp"");

    modelBuilder.Entity<Product>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).HasMaxLength(100);
    });
}";

            // Act
            var scaffolderType = typeof(DatabaseScaffolder);
            var enhanceMethod = scaffolderType.GetMethod("EnhanceOnModelCreating",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var result = (string)enhanceMethod.Invoke(null, new object[] { onModelCreating, null });

            // Assert
            result.Should().Contain("modelBuilder.HasPostgresExtension(\"uuid-ossp\");", 
                "semicolon should be preserved for extension call");
            result.Should().Contain("entity.HasKey(e => e.Id);", 
                "semicolon should be preserved for HasKey");
            result.Should().Contain(".HasMaxLength(100);", 
                "semicolon should be preserved for HasMaxLength");
            result.Should().Contain("});", 
                "closing brace and semicolon should be preserved for entity configuration");
        }

        /// <summary>
        /// Tests that proper indentation is maintained in the enhanced OnModelCreating.
        /// </summary>
        [TestMethod]
        public void EnhanceOnModelCreating_ShouldMaintainProperIndentation()
        {
            // Arrange
            var onModelCreating = @"protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Category>(entity =>
    {
        entity.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasComment(""The unique identifier"");
        
        entity.Property(e => e.Name)
            .HasMaxLength(50)
            .IsRequired();
    });
}";

            // Act
            var scaffolderType = typeof(DatabaseScaffolder);
            var enhanceMethod = scaffolderType.GetMethod("EnhanceOnModelCreating",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var result = (string)enhanceMethod.Invoke(null, new object[] { onModelCreating, null });

            // Assert
            var lines = result.Split('\n');
            
            // Check method declaration indentation (4 spaces)
            lines[0].Should().StartWith("    protected override");
            
            // Check opening brace indentation (4 spaces)
            lines[1].Should().Be("    {");
            
            // Check entity configuration indentation (8 spaces)
            result.Should().Contain("        modelBuilder.Entity<Category>");
            
            // Check entity.IgnoreTrackingFields indentation (12 spaces)
            result.Should().Contain("            entity.IgnoreTrackingFields();");
            
            // Check property configuration continuation indentation (16 spaces)
            result.Should().Contain("                .ValueGeneratedNever()");
            result.Should().Contain("                .HasComment(\"The unique identifier\");");
        }

        /// <summary>
        /// Tests that HasColumnName is properly injected with correct formatting.
        /// </summary>
        [TestMethod]
        public void EnhanceOnModelCreating_WithPropertyOverrides_ShouldHaveCorrectFormatting()
        {
            // Arrange
            var onModelCreating = @"protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<NationalStockNumber>(entity =>
    {
        entity.Property(e => e.Niin)
            .HasMaxLength(9)
            .IsRequired();
        
        entity.Property(e => e.Fsc)
            .HasMaxLength(4);
    });
}";

            var propertyOverrides = new Dictionary<string, Dictionary<string, string>>
            {
                ["NationalStockNumber"] = new Dictionary<string, string>
                {
                    ["NIIN"] = "Niin",
                    ["FSC"] = "Fsc"
                }
            };

            // Act
            var scaffolderType = typeof(DatabaseScaffolder);
            var enhanceMethod = scaffolderType.GetMethod("EnhanceOnModelCreating",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var result = (string)enhanceMethod.Invoke(null, new object[] { onModelCreating, propertyOverrides });

            // Assert
            // Check that HasColumnName is added with proper indentation
            result.Should().Contain("entity.Property(e => e.Niin)");
            result.Should().Contain("                    .HasColumnName(\"NIIN\")");
            result.Should().Contain("                .HasMaxLength(9)");
            result.Should().Contain("                .IsRequired();");
            
            // Check formatting for second property
            result.Should().Contain("entity.Property(e => e.Fsc)");
            result.Should().Contain("                    .HasColumnName(\"FSC\")");
            result.Should().Contain("                .HasMaxLength(4);");
            
            // Verify semicolons are present
            var semicolonCount = result.Split(';').Length - 1;
            semicolonCount.Should().BeGreaterThan(3, "multiple semicolons should be present");
        }

        /// <summary>
        /// Tests that complex multi-entity configurations maintain proper formatting.
        /// </summary>
        [TestMethod]
        public void EnhanceOnModelCreating_ComplexConfiguration_ShouldMaintainStructure()
        {
            // Arrange
            var onModelCreating = @"protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasPostgresExtension(""uuid-ossp"");

    modelBuilder.Entity<Part>(entity =>
    {
        entity.HasKey(e => e.Id).HasName(""PK_Parts_Id"");
        
        entity.Property(e => e.Id).ValueGeneratedNever();
        
        entity.HasOne(d => d.Parent)
            .WithMany(p => p.InverseParent)
            .HasForeignKey(d => d.ParentId)
            .HasConstraintName(""FK_Parts_Parent"");
    });

    modelBuilder.Entity<User>(entity =>
    {
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();
    });

    OnModelCreatingPartial(modelBuilder);
}";

            // Act
            var scaffolderType = typeof(DatabaseScaffolder);
            var enhanceMethod = scaffolderType.GetMethod("EnhanceOnModelCreating",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var result = (string)enhanceMethod.Invoke(null, new object[] { onModelCreating, null });

            // Assert
            // Check structure preservation
            result.Should().Contain("modelBuilder.HasPostgresExtension(\"uuid-ossp\");");
            result.Should().Contain("entity.HasKey(e => e.Id).HasName(\"PK_Parts_Id\");");
            result.Should().Contain(".WithMany(p => p.Children)"); // InverseParent should be replaced
            result.Should().Contain(".HasConstraintName(\"FK_Parts_Parent\");");
            result.Should().Contain("OnModelCreatingPartial(modelBuilder);");
            
            // Check that each entity has IgnoreTrackingFields
            var ignoreCount = System.Text.RegularExpressions.Regex.Matches(result, @"entity\.IgnoreTrackingFields\(\);").Count;
            ignoreCount.Should().Be(2, "both entities should have IgnoreTrackingFields");
            
            // Check overall structure with closing braces
            result.Should().Contain("    });"); // Entity configuration closing
            result.Should().EndWith("    }\r\n"); // Method closing
        }

        #endregion

    }

}