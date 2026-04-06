using CloudNimble.EasyAF.CodeGen;

namespace CloudNimble.EasyAF.Analyzers.EF6.SourceGeneration
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class SourceGeneratorBase
    {

        /// <summary>
        /// 
        /// </summary>
        public EdmxLoader EdmxLoader { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public SourceGeneratorSettings Settings { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edmxLoader"></param>
        /// <param name="settings">The generator settings.</param>
        public SourceGeneratorBase(EdmxLoader edmxLoader, SourceGeneratorSettings settings)
        {
            EdmxLoader = edmxLoader;
            Settings = settings;
        }

    }

}
