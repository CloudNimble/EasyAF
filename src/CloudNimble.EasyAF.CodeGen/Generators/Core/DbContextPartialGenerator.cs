using CloudNimble.EasyAF.CodeGen.Generators.Base;
using CloudNimble.EasyAF.CodeGen.Legacy;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Linq;

namespace CloudNimble.EasyAF.CodeGen.Generators.Core
{

    /// <summary>
    /// 
    /// </summary>
    public class DbContextPartialGenerator : ContainerGeneratorBase
    {

        #region Fields

        private readonly string _onModelCreatingBody = string.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// /
        /// </summary>
        public string FileName { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extraUsings"></param>
        /// <param name="contextNamespace"></param>
        /// <param name="container"></param>
        /// <param name="onModelCreatingBody"></param>
        /// <param name="edmxPath"></param>
        public DbContextPartialGenerator(List<string> extraUsings, string contextNamespace, EntityContainer container, string onModelCreatingBody, string edmxPath)
            : base(extraUsings, contextNamespace, container)
        {
            FileName = Path.GetFileNameWithoutExtension(edmxPath);
            _onModelCreatingBody = onModelCreatingBody;
            AddUsings();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        public override void Generate()
        {
            if (IsGenerated) return;
            Header();
            WriteUsings();
            NamespaceBegin(Namespace);
            ClassBegin(CodeGenerationTools.DbContextClassDeclaration(EntityContainer), MetadataTools.Comment(EntityContainer));
            WriteProperties();
            WriteConstructors();
            WriteOverrides();
            ClassEnd();
            NamespaceEnd();
            IsGenerated = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string WriteFile(string directory = null)
        {
            return WriteFile(CodeGenerationTools.Escape(EntityContainer), directory);
        }

        #endregion

        #region Private Methods

        internal void WriteConstructors()
        {
            RegionBegin("Constructors");
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"///");
            _writer.WriteLine("/// </summary>");


            if (!string.IsNullOrWhiteSpace(_onModelCreatingBody))
            {
                _writer.WriteLine("/// <param name=\"options\"></param>");
                _writer.WriteLine($"{Accessibility.ForType(EntityContainer)} {CodeGenerationTools.Escape(EntityContainer)}(DbContextOptions<{CodeGenerationTools.Escape(EntityContainer)}> options)");
                _writer.WriteLine($"    : base(options)");
                _writer.WriteLine("{");
                _writer.WriteLine("}");
            }
            else
            {
                _writer.WriteLine($"{Accessibility.ForType(EntityContainer)} {CodeGenerationTools.Escape(EntityContainer)}() : base(\"name={EntityContainer.Name}\")");
                _writer.WriteLine("{");
                if (MetadataTools.IsLazyLoadingEnabled(EntityContainer))
                {
                    _writer.Indent++;
                    _writer.WriteLine("this.Configuration.LazyLoadingEnabled = false;");
                    _writer.Indent--;
                }
                _writer.WriteLine("}");
                _writer.WriteLine();

                _writer.WriteLine("/// <summary>");
                _writer.WriteLine($"/// Creates a new <see cref=\"{CodeGenerationTools.Escape(EntityContainer)}\"/> instance for a given connection string.");
                _writer.WriteLine("/// </summary>");
                _writer.WriteLine("/// <param name=\"sqlConnectionString\">A SqlClient connection string that does not have EntityClient metadata.</param>");
                _writer.WriteLine($"{Accessibility.ForType(EntityContainer)} {CodeGenerationTools.Escape(EntityContainer)}(string sqlConnectionString) : base(GetEntityConnection(sqlConnectionString), true)");
                _writer.WriteLine("{");
                _writer.WriteLine("}");
            }
            _writer.WriteLine();
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteOverrides()
        {
            if (!string.IsNullOrWhiteSpace(_onModelCreatingBody))
            {
                RegionBegin("Partial Methods");
                _writer.WriteLine("/// <summary>");
                _writer.WriteLine("/// Provides a hook for configuring the model during the creation process.");
                _writer.WriteLine("/// </summary>");
                _writer.WriteLine("/// <remarks>This partial method allows additional customization of the model configuration ");
                _writer.WriteLine("/// beyond the default setup. Implement this method in a partial class to define  custom behavior or mappings");
                _writer.WriteLine("/// for the model.</remarks>");
                _writer.WriteLine("/// <param name=\"modelBuilder\">The <see cref=\"ModelBuilder\"/> instance used to configure the model.</param>");
                _writer.WriteLine("partial void OnModelCreatingPartial(ModelBuilder modelBuilder);");
                _writer.WriteLine();
                RegionEnd();

                RegionBegin("Private Methods");
                _writer.Indent--;
                _writer.WriteLine(_onModelCreatingBody);
                _writer.WriteLine();
                RegionEnd();
            }
            else
            {
                RegionBegin("Private Methods");
                _writer.WriteLine("/// <summary>");
                _writer.WriteLine($"///");
                _writer.WriteLine("/// </summary>");
                _writer.WriteLine("protected override void OnModelCreating(DbModelBuilder modelBuilder)");
                _writer.WriteLine("{");
                _writer.Indent++;
                _writer.WriteLine("throw new UnintentionalCodeFirstException();");
                _writer.Indent--;
                _writer.WriteLine("}");
                _writer.WriteLine();
                _writer.WriteLine("/// <summary>");
                _writer.WriteLine("/// ");
                _writer.WriteLine("/// </summary>");
                _writer.WriteLine("/// <param name=\"sqlConnectionString\">A SqlClient connection string that does not have EntityClient metadata.</param>");
                _writer.WriteLine($"/// <returns>an <see cref=\"EntityConnection\" /> object populated with the default values for an {CodeGenerationTools.Escape(EntityContainer)} EF6 connection.</returns>");
                _writer.WriteLine("private static EntityConnection GetEntityConnection(string sqlConnectionString)");
                _writer.WriteLine("{");
                _writer.Indent++;
                _writer.WriteLine("var entityBuilder = new EntityConnectionStringBuilder()");
                _writer.WriteLine("{");
                _writer.Indent++;
                _writer.WriteLine("Provider = \"Microsoft.Data.SqlClient\",");
                _writer.WriteLine("ProviderConnectionString = sqlConnectionString,");
                _writer.WriteLine($"Metadata = @\"res://*/{FileName}.csdl|res://*/{FileName}.ssdl|res://*/{FileName}.msl\",");
                _writer.Indent--;
                _writer.WriteLine("};");
                _writer.WriteLine("return new EntityConnection(entityBuilder.ToString());");
                _writer.Indent--;
                _writer.WriteLine("}");
                _writer.WriteLine();
                RegionEnd();

            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteProperties()
        {
            RegionBegin("Public Properties");
            foreach (var entitySet in EntityContainer.BaseEntitySets.OfType<EntitySet>().OrderBy(c => c.Name))
            {
                _writer.WriteLine("/// <summary>");
                _writer.WriteLine($"/// {MetadataTools.Comment(entitySet)}");
                _writer.WriteLine("/// </summary>");
                _writer.WriteLine(CodeGenerationTools.DbSet(entitySet));
                _writer.WriteLine();
            }
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void AddUsings()
        {
            if (!string.IsNullOrWhiteSpace(_onModelCreatingBody))
            {
                ExtraUsings.Add("Microsoft.EntityFrameworkCore");
                ExtraUsings.Add("Microsoft.EntityFrameworkCore.Metadata.Builders");
            }
            else
            {
                ExtraUsings.Add("System.Data.Entity");
                ExtraUsings.Add("System.Data.Entity.Core.EntityClient");
                ExtraUsings.Add("System.Data.Entity.Infrastructure");
            }
        }

        #endregion

    }

}
