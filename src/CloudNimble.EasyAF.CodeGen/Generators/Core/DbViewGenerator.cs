using CloudNimble.EasyAF.CodeGen.Generators.Base;
using Microsoft.DbContextPackage.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace CloudNimble.EasyAF.CodeGen.Generators.Core
{

    /// <summary>
    /// 
    /// </summary>
    public class DbViewGenerator : ContainerGeneratorBase
    {

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public StorageMappingItemCollection Mappings { get; private set; }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extraUsings"></param>
        /// <param name="namespaceName"></param>
        /// <param name="container"></param>
        /// <param name="mappings"></param>
        public DbViewGenerator(List<string> extraUsings, string namespaceName, EntityContainer container, StorageMappingItemCollection mappings) : base(extraUsings, namespaceName, container)
        {
            Mappings = mappings;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Generate()
        {
            if (IsGenerated) return;
            Header();

            var csvg = new CSharpViewGenerator();
            var errors = new List<EdmSchemaError>();

            var contextTypeName = (string.IsNullOrEmpty(Namespace) ? string.Empty : Namespace + ".") + EntityContainer.Name;
            var views = Mappings.GenerateViews(errors);

            if (errors.Any(c => c.Severity == EdmSchemaErrorSeverity.Error))
            {
                Console.WriteLine("Could not generate EF6 Views, there was an error with the data model.");
                return;
            }

            csvg.ContextTypeName = contextTypeName;
            csvg.MappingHashValue = Mappings.ComputeMappingHashValue();
            csvg.Views = views;

            _writer.Write(csvg.TransformText());

            IsGenerated = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string WriteFile(string directory = null)
        {
            return WriteFile($"{EntityContainer.Name}.Views", directory);
        }

    }

}
