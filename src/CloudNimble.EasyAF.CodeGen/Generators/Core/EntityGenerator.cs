using CloudNimble.EasyAF.CodeGen.Generators.Base;
using CloudNimble.EasyAF.CodeGen.Legacy;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace CloudNimble.EasyAF.CodeGen.Generators.Core
{

    /// <summary>
    /// 
    /// </summary>
    public class EntityGenerator : EntityGeneratorBase
    {

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extraUsings"></param>
        /// <param name="entityNamespace"></param>
        /// <param name="entity"></param>
        public EntityGenerator(List<string> extraUsings, string entityNamespace, EntityComposition entity) : base(extraUsings, entityNamespace, entity)
        {
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
            ClassBegin(CodeGenerationTools.EntityClassDeclaration(Entity), MetadataTools.Comment(Entity.EntityType));
            WriteFields();
            WriteProperties();
            WriteConstructors();
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
            return WriteFile(Entity.EntityType.Name, directory);
        }

        #endregion

        #region Private Methods

        internal void WriteConstructors()
        {
            RegionBegin("Constructors");

            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// ");
            _writer.WriteLine("/// </summary>");
            _writer.WriteLine($"public {CodeGenerationTools.Escape(Entity.EntityType)}()");
            _writer.WriteLine("{");
            _writer.WriteLine("}");
            _writer.WriteLine();

            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteFields()
        {
            RegionBegin("Private Members");
            foreach (var property in Entity.SimpleProperties.OrderBy(c => c.Name))
            {
                _writer.WriteLine($"private {CodeGenerationTools.GetTypeName(property.TypeUsage).Replace("System.", "")} {CodeGenerationTools.FieldName(property)};");
            }
            foreach (var property in Entity.ComplexProperties.OrderBy(c => c.Name))
            {
                _writer.WriteLine($"private {CodeGenerationTools.GetTypeName(property.TypeUsage).Replace("System.", "")} {CodeGenerationTools.FieldName(property)};");
            }
            foreach (var property in Entity.NavigationProperties.OrderBy(c => c.Name))
            {
                _writer.WriteLine($"private {CodeGenerationTools.GetTypeName(property.TypeUsage).Replace("System.", "")} {CodeGenerationTools.FieldName(property)};");
            }
            _writer.WriteLine();
            RegionEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void WriteProperties()
        {
            RegionBegin("Public Properties");
            Entity.SimpleProperties.ToList().ForEach(c => WriteProperty(c));
            Entity.ComplexProperties.ForEach(c => WriteProperty(c));
            Entity.NavigationProperties.ForEach(c => WriteProperty(c));
            RegionEnd();
        }

        internal void WriteProperty(EdmProperty property)
        {
            WriteProperty(Accessibility.ForProperty(property),
                          CodeGenerationTools.Escape(property),
                          CodeGenerationTools.FieldName(property),
                          CodeGenerationTools.GetTypeName(property.TypeUsage).Replace("System.", ""),
                          MetadataTools.Comment(property),
                          property.TypeUsage.Facets.ToList());
        }

        internal void WriteProperty(NavigationProperty property)
        {
            WriteProperty(Accessibility.ForProperty(property),
                          CodeGenerationTools.Escape(property),
                          CodeGenerationTools.FieldName(property),
                          CodeGenerationTools.GetTypeName(property.TypeUsage).Replace("System.", ""),
                          MetadataTools.Comment(property),
                          property.TypeUsage.Facets.ToList());
        }

        internal void WriteProperty(string accessibility, string propertyName, string fieldName, string type, string comment, List<Facet> facets)
        {
            _writer.WriteLine("/// <summary>");
            _writer.WriteLine($"/// {comment}");
            _writer.WriteLine("/// </summary>");

            var facet = facets.FirstOrDefault(c => c.Name == "MaxLength" && c.Value is not null && c.IsUnbounded == false);
            if (facet is not null)
            {
                _writer.WriteLine($"[StringLength({facet.Value})]");
            }

            _writer.WriteLine($"{accessibility} {type} {propertyName}");
            _writer.WriteLine("{");
            _writer.Indent++;
            _writer.WriteLine($"get => {fieldName};");
            _writer.WriteLine($"set => Set(() => {propertyName}, ref {fieldName}, value);");
            _writer.Indent--;
            _writer.WriteLine("}");
            _writer.WriteLine();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void AddUsings()
        {
            ExtraUsings.Add("CloudNimble.EasyAF.Core");
            ExtraUsings.Add("System");
            ExtraUsings.Add("System.Collections.Generic");
            ExtraUsings.Add("System.Collections.ObjectModel");
            ExtraUsings.Add("System.ComponentModel.DataAnnotations");
            ExtraUsings.Add("System.ComponentModel.DataAnnotations.Schema");
        }

        #endregion

    }

}
