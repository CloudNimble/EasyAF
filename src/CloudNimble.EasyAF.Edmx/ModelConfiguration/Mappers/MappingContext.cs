// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using ModelConfig = System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration;


namespace System.Data.Entity.ModelConfiguration.Mappers
{
    internal sealed class MappingContext
    {
        private readonly ModelConfig _modelConfiguration;
        private readonly ConventionsConfiguration _conventionsConfiguration;
        private readonly EdmModel _model;
        private readonly AttributeProvider _attributeProvider;
        private readonly DbModelBuilderVersion _modelBuilderVersion;

        public MappingContext(
            ModelConfig modelConfiguration,
            ConventionsConfiguration conventionsConfiguration,
            EdmModel model,
            DbModelBuilderVersion modelBuilderVersion = DbModelBuilderVersion.Latest,
            AttributeProvider attributeProvider = null)
        {
            DebugCheck.NotNull(modelConfiguration);
            DebugCheck.NotNull(conventionsConfiguration);
            DebugCheck.NotNull(model);

            _modelConfiguration = modelConfiguration;
            _conventionsConfiguration = conventionsConfiguration;
            _model = model;
            _modelBuilderVersion = modelBuilderVersion;
            _attributeProvider = attributeProvider ?? new AttributeProvider();
        }

        public ModelConfig ModelConfiguration
        {
            get { return _modelConfiguration; }
        }

        public ConventionsConfiguration ConventionsConfiguration
        {
            get { return _conventionsConfiguration; }
        }

        public EdmModel Model
        {
            get { return _model; }
        }

        public AttributeProvider AttributeProvider
        {
            get { return _attributeProvider; }
        }

        public DbModelBuilderVersion ModelBuilderVersion
        {
            get { return _modelBuilderVersion; }
        }
    }
}
