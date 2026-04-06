// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Edm
{
    internal static class FunctionParameterExtensions
    {
        public static object GetConfiguration(this FunctionParameter functionParameter)
        {
            DebugCheck.NotNull(functionParameter);

            return functionParameter.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this FunctionParameter functionParameter, object configuration)
        {
            DebugCheck.NotNull(functionParameter);

            functionParameter.GetMetadataProperties().SetConfiguration(configuration);
        }
    }
}
