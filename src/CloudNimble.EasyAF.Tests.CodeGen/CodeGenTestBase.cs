using CloudNimble.EasyAF.CodeGen;
using System.Data.Entity;
using System.IO;
using System.Text.RegularExpressions;

namespace CloudNimble.EasyAF.Tests.CodeGen
{
    public abstract partial class CodeGenTestBase
    {

        internal const string RootPath = @"..\..\..\..\";
        internal const string ProjectPath = RootPath + @"CloudNimble.EasyAF.Tests.CodeGen\";
        internal const string ModelPath = RootPath + @"CloudNimble.EasyAF.Tests.Shared\EntityModel.edmx";

        static CodeGenTestBase()
        {
            DbConfiguration.SetConfiguration(new EF6Configuration());
            //CloudNimble.EasyAF.Edmx.InMemoryDb.Provider.EffortProviderConfiguration.RegisterProvider();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        internal static void WriteFile(string path, string content)
        {
            File.WriteAllText(Path.Combine(ProjectPath, path), content);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string GetDirectory(string path)
        {
            return Path.GetDirectoryName(path.StartsWith(RootPath) ? path : Path.Combine(RootPath, path));
        }

#if NET8_0_OR_GREATER
        [GeneratedRegex(@"Date Generated: \d{1,2}/\d{1,2}/\d{4} \d{1,2}:\d{2}:\d{2} [APM]{2}", RegexOptions.Compiled)]
        internal static partial Regex TimestampRegex();
#else
        internal static Regex TimestampRegex()
        {
            return new Regex(@"Date Generated: \d{1,2}/\d{1,2}/\d{4} \d{1,2}:\d{2}:\d{2} [APM]{2}");
        }
#endif
    }

}
