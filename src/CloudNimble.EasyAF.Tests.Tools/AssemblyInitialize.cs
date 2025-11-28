using CloudNimble.EasyAF.MSBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudNimble.EasyAF.Tests.Tools
{

    /// <summary>
    /// Assembly-level test initialization that runs before any tests.
    /// </summary>
    [TestClass]
    public static class AssemblyInitialize
    {

        /// <summary>
        /// Registers MSBuild before any tests run. This must happen before any
        /// Microsoft.Build types are loaded so MSBuildLocator can redirect
        /// assembly resolution to the SDK.
        /// </summary>
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            MSBuildProjectManager.EnsureMSBuildRegistered();
        }

    }

}
