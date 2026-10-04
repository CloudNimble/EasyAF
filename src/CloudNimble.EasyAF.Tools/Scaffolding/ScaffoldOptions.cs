using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CloudNimble.EasyAF.Tools.Scaffolding
{

    /// <summary>
    /// The validated inputs for <c>dotnet easyaf new</c>: the solution namespace, target framework, and which optional
    /// projects to include.
    /// </summary>
    public sealed class ScaffoldOptions
    {

        #region Fields

        /// <summary>
        /// The version range every EasyAF package reference in a new solution uses: the same major version as this tool.
        /// </summary>
        public const string EasyAFPackageVersion = "5.*";

        private static readonly int[] SupportedMajorVersions = [10, 11];

        #endregion

        #region Properties

        /// <summary>
        /// Gets the first namespace segment (for example <c>CloudNimble</c> for <c>CloudNimble.Contoso</c>), used for package metadata.
        /// </summary>
        public string Company { get; }

        /// <summary>
        /// Gets the .NET major version every project targets (for example <c>11</c>).
        /// </summary>
        public int DotNetVersion { get; }

        /// <summary>
        /// Gets a value indicating whether the <c>.Api</c> project (and its test project) is created.
        /// </summary>
        public bool IncludeApi { get; }

        /// <summary>
        /// Gets a value indicating whether the <c>.MessageBus.*</c> projects are created.
        /// </summary>
        public bool IncludeMessageBus { get; }

        /// <summary>
        /// Gets a value indicating whether the <c>.MessageBus.Runtime</c> project is created. Always <see langword="false"/>
        /// when <see cref="IncludeMessageBus"/> is <see langword="false"/>.
        /// </summary>
        public bool IncludeRuntime { get; }

        /// <summary>
        /// Gets the version that Microsoft.Extensions and EF Core packages float to for <see cref="TargetFramework"/>.
        /// </summary>
        public string MicrosoftPackageVersion { get; }

        /// <summary>
        /// Gets the solution namespace (for example <c>CloudNimble.Contoso</c>); every project name starts with it.
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// Gets the last namespace segment (for example <c>Contoso</c>), used in class, configuration, and WebJob names.
        /// </summary>
        public string Product { get; }

        /// <summary>
        /// Gets the target framework moniker every project uses (for example <c>net11.0</c>).
        /// </summary>
        public string TargetFramework { get; }

        /// <summary>
        /// Gets a value indicating whether Azure WebJob publish metadata is stamped on the <c>.MessageBus.Runtime</c> project.
        /// </summary>
        public bool WebJob { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaffoldOptions"/> class.
        /// </summary>
        /// <param name="namespace">The solution namespace; every dot-separated segment must be a C# identifier.</param>
        /// <param name="dotNetVersion">The .NET major version to target: <c>10</c> or <c>11</c>.</param>
        /// <param name="includeApi">Whether to create the <c>.Api</c> project.</param>
        /// <param name="includeMessageBus">Whether to create the <c>.MessageBus.*</c> projects.</param>
        /// <param name="includeRuntime">Whether to create the <c>.MessageBus.Runtime</c> project.</param>
        /// <param name="webJob">Whether to stamp Azure WebJob publish metadata on the runtime.</param>
        /// <exception cref="ArgumentException">Thrown when an argument is invalid or the flags conflict.</exception>
        public ScaffoldOptions(string @namespace, int dotNetVersion = 11, bool includeApi = true, bool includeMessageBus = true, bool includeRuntime = true, bool webJob = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(@namespace, nameof(@namespace));

            Namespace = @namespace.Trim();
            var segments = Namespace.Split('.');
            ValidateNamespace(Namespace, segments);

            if (!SupportedMajorVersions.Contains(dotNetVersion))
            {
                throw new ArgumentException($".NET {dotNetVersion} is not supported. Use {string.Join(" or ", SupportedMajorVersions)}.", nameof(dotNetVersion));
            }

            IncludeRuntime = includeMessageBus && includeRuntime;
            if (webJob && !IncludeRuntime)
            {
                throw new ArgumentException("--webjob stamps the MessageBus.Runtime project, so it cannot be combined with --no-runtime or --no-messagebus.", nameof(webJob));
            }

            DotNetVersion = dotNetVersion;
            TargetFramework = $"net{dotNetVersion}.0";
            IncludeApi = includeApi;
            IncludeMessageBus = includeMessageBus;
            WebJob = webJob;
            Company = segments[0];
            Product = segments[^1];
            MicrosoftPackageVersion = dotNetVersion switch
            {
                11 => "11.*-*",
                _ => "10.*",
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the token values the embedded scaffold resources are rendered with.
        /// </summary>
        /// <param name="sdkVersion">The .NET SDK version to pin in <c>global.json</c>.</param>
        /// <param name="year">The copyright year.</param>
        /// <returns>The tokens keyed by placeholder name.</returns>
        public IReadOnlyDictionary<string, string> CreateTokens(string sdkVersion, int year)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sdkVersion, nameof(sdkVersion));

            return new Dictionary<string, string>
            {
                ["Company"] = Company,
                ["EasyAFPackageVersion"] = EasyAFPackageVersion,
                ["Namespace"] = Namespace,
                ["Product"] = Product,
                ["SdkVersion"] = sdkVersion,
                ["Year"] = year.ToString(CultureInfo.InvariantCulture),
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Ensures every namespace segment is a C# identifier that is not a keyword and does not start with <c>Test</c>.
        /// </summary>
        /// <param name="namespace">The full namespace, for error messages.</param>
        /// <param name="segments">The dot-separated segments.</param>
        /// <exception cref="ArgumentException">Thrown when a segment is invalid.</exception>
        private static void ValidateNamespace(string @namespace, string[] segments)
        {
            foreach (var segment in segments)
            {
                if (!SyntaxFacts.IsValidIdentifier(segment) || SyntaxFacts.GetKeywordKind(segment) != SyntaxKind.None)
                {
                    throw new ArgumentException($"'{@namespace}' is not a valid C# namespace: '{segment}' is not a valid identifier.", nameof(@namespace));
                }

                // EasyAF detects project types by name and treats anything containing ".Test" as a test project.
                if (segment.StartsWith("Test", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        $"'{@namespace}' cannot be used: the segment '{segment}' starts with 'Test', which would make EasyAF treat every project as a test project.",
                        nameof(@namespace));
                }
            }
        }

        #endregion

    }

}
