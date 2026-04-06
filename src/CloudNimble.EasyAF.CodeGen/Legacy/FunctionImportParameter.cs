using CloudNimble.EasyAF.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;

namespace CloudNimble.EasyAF.CodeGen.Legacy
{
    /// <summary>
    /// Responsible for collecting together the actual method parameters
    /// and the parameters that need to be sent to the Execute method.
    /// </summary>
    public class FunctionImportParameter
    {

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public FunctionParameter Source { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RawFunctionParameterName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FunctionParameterName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FunctionParameterType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LocalVariableName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RawClrTypeName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ExecuteParameterName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string EsqlParameterName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool NeedsLocalVariable { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsNullableOfT { get; set; }

        #endregion

        /// <summary>
        /// Creates a set of FunctionImportParameter objects from the parameters passed in.
        /// </summary>
        public static IEnumerable<FunctionImportParameter> Create(IEnumerable<FunctionParameter> parameters)
        {
            Ensure.ArgumentNotNull(parameters, nameof(parameters));

            var unique = new UniqueIdentifierService();
            var importParameters = new List<FunctionImportParameter>();
            foreach (var parameter in parameters)
            {
                var importParameter = new FunctionImportParameter
                {
                    Source = parameter,
                    RawFunctionParameterName = unique.AdjustIdentifier(CodeGenerationTools.CamelCase(parameter.Name))
                };
                importParameter.FunctionParameterName = CodeGenerationTools.Escape(importParameter.RawFunctionParameterName);
                if (parameter.Mode == ParameterMode.In)
                {
                    var typeUsage = parameter.TypeUsage;
                    importParameter.NeedsLocalVariable = true;
                    importParameter.FunctionParameterType = CodeGenerationTools.GetTypeName(typeUsage);
                    importParameter.EsqlParameterName = parameter.Name;
                    var clrType = MetadataTools.UnderlyingClrType(parameter.TypeUsage.EdmType);
                    importParameter.RawClrTypeName = typeUsage.EdmType is EnumType ? CodeGenerationTools.GetTypeName(typeUsage.EdmType) : CodeGenerationTools.Escape(clrType);
                    importParameter.IsNullableOfT = clrType.IsValueType;
                }
                else
                {
                    importParameter.NeedsLocalVariable = false;
                    importParameter.FunctionParameterType = "ObjectParameter";
                    importParameter.ExecuteParameterName = importParameter.FunctionParameterName;
                }
                importParameters.Add(importParameter);
            }

            // we save the local parameter uniquification for a second pass to make the visible parameters
            // as pretty and sensible as possible
            for (var i = 0; i < importParameters.Count; i++)
            {
                var importParameter = importParameters[i];
                if (importParameter.NeedsLocalVariable)
                {
                    importParameter.LocalVariableName = unique.AdjustIdentifier(importParameter.RawFunctionParameterName + "Parameter");
                    importParameter.ExecuteParameterName = importParameter.LocalVariableName;
                }
            }

            return importParameters;
        }

        //
        // Class to create unique variables within the same scope
        //
        private sealed class UniqueIdentifierService
        {
            private readonly HashSet<string> _knownIdentifiers;

            public UniqueIdentifierService()
            {
                _knownIdentifiers = new HashSet<string>(StringComparer.Ordinal);
            }

            /// <summary>
            /// Given an identifier, makes it unique within the scope by adding
            /// a suffix (1, 2, 3, ...), and returns the adjusted identifier.
            /// </summary>
            public string AdjustIdentifier(string identifier)
            {
                // find a unique name by adding suffix as necessary
                var numberOfConflicts = 0;
                var adjustedIdentifier = identifier;

                while (!_knownIdentifiers.Add(adjustedIdentifier))
                {
                    ++numberOfConflicts;
                    adjustedIdentifier = identifier + numberOfConflicts.ToString(CultureInfo.InvariantCulture);
                }

                return adjustedIdentifier;
            }
        }
    }
}
