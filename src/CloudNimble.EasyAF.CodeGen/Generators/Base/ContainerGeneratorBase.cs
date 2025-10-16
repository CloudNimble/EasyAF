using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace CloudNimble.EasyAF.CodeGen.Generators.Base
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class ContainerGeneratorBase : CodeGeneratorBase
    {

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public EntityContainer EntityContainer { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extraUsings"></param>
        /// <param name="namespaceName"></param>
        /// <param name="container"></param>
        public ContainerGeneratorBase(List<string> extraUsings, string namespaceName, EntityContainer container) : base(extraUsings, namespaceName)
        {
            EntityContainer = container;
        }

        #endregion

    }
}
