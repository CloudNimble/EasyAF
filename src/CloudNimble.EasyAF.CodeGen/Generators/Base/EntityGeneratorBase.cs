using System.Collections.Generic;

namespace CloudNimble.EasyAF.CodeGen.Generators.Base
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class EntityGeneratorBase : CodeGeneratorBase
    {

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public EntityComposition Entity { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extraUsings"></param>
        /// <param name="namespaceName"></param>
        /// <param name="entity"></param>
        public EntityGeneratorBase(List<string> extraUsings, string namespaceName, EntityComposition entity) : base(extraUsings, namespaceName)
        {
            Entity = entity;
        }

        #endregion

    }

}
