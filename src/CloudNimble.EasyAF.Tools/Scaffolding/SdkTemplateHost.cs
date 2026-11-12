using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Abstractions.TemplatePackage;
using Microsoft.TemplateEngine.Edge;
using Microsoft.TemplateEngine.Edge.Template;
using Microsoft.TemplateEngine.IDE;
using Microsoft.TemplateEngine.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DefaultTemplateEngineHost = Microsoft.TemplateEngine.Edge.DefaultTemplateEngineHost;

namespace CloudNimble.EasyAF.Tools.Scaffolding
{

    /// <summary>
    /// Instantiates the .NET SDK's built-in C# project templates (<c>classlib</c>, <c>web</c>, <c>console</c>)
    /// in-process through Microsoft.TemplateEngine, without spawning <c>dotnet new</c>.
    /// </summary>
    /// <remarks>
    /// Template packages are discovered the same way the SDK's own <c>BuiltInTemplatePackageProvider</c> finds them:
    /// <c>{dotnetRoot}/templates/{version}/*.nupkg</c>, where <c>{version}</c> is the highest folder whose major version
    /// matches the requested target framework. Template post-actions (such as restore) are not executed.
    /// </remarks>
    public sealed class SdkTemplateHost : IDisposable
    {

        #region Fields

        private const string CommonProjectTemplatesPrefix = "microsoft.dotnet.common.projecttemplates.";
        private const string CSharpLanguage = "C#";
        private const string FrameworkParameterName = "Framework";
        private const string ProjectTemplateType = "project";

        private Bootstrapper _bootstrapper;
        private DefaultTemplateEngineHost _host;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the version of the SDK that supplied the templates in <see cref="TemplateFolder"/> (for example, <c>10.0.401</c>),
        /// or <see langword="null"/> when it can't be determined.
        /// </summary>
        /// <remarks>
        /// The SDK installs its common project templates as <c>microsoft.dotnet.common.projecttemplates.{Major}.{Minor}.{SdkVersion}.nupkg</c>,
        /// so the version is read from that package name.
        /// </remarks>
        public string SdkVersion { get; }

        /// <summary>
        /// Gets the resolved template version folder (for example, <c>C:\Program Files\dotnet\templates\10.0.12</c>),
        /// or <see langword="null"/> when <see cref="TemplatesRoot"/> has no folder for the target framework's major version.
        /// </summary>
        public string TemplateFolder { get; }

        /// <summary>
        /// Gets the folder containing versioned template package folders (normally <c>{dotnetRoot}/templates</c>).
        /// </summary>
        public string TemplatesRoot { get; }

        /// <summary>
        /// Gets the target framework moniker projects are created for (for example, <c>net10.0</c>).
        /// </summary>
        public string TargetFramework { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SdkTemplateHost"/> class.
        /// </summary>
        /// <param name="templatesRoot">The folder containing versioned template package folders.</param>
        /// <param name="targetFramework">The target framework moniker projects will be created for.</param>
        public SdkTemplateHost(string templatesRoot, string targetFramework)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(templatesRoot, nameof(templatesRoot));
            ArgumentException.ThrowIfNullOrWhiteSpace(targetFramework, nameof(targetFramework));

            TemplatesRoot = Path.GetFullPath(templatesRoot);
            TargetFramework = targetFramework;
            TemplateFolder = ResolveTemplateFolder(TemplatesRoot, GetMajorVersion(targetFramework));
            SdkVersion = ResolveSdkVersion(TemplateFolder);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Finds which of the given C# project templates are not available, so a caller can fail before creating anything.
        /// </summary>
        /// <param name="shortNames">The template short names to look for, such as <c>classlib</c>, <c>web</c>, and <c>console</c>.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The short names with no matching C# project template, in the order given. Empty when all are available.</returns>
        public async Task<IReadOnlyList<string>> GetMissingTemplatesAsync(IEnumerable<string> shortNames, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(shortNames, nameof(shortNames));

            var requested = shortNames.ToList();
            if (TemplateFolder is null)
            {
                return requested;
            }

            var templates = await GetBootstrapper().GetTemplatesAsync(cancellationToken).ConfigureAwait(false);
            return [.. requested.Where(shortName => FindTemplate(templates, shortName) is null)];
        }

        /// <summary>
        /// Creates a project from an SDK template.
        /// </summary>
        /// <param name="shortName">The template short name: <c>classlib</c>, <c>web</c>, or <c>console</c>.</param>
        /// <param name="projectName">The project name, which becomes the .csproj file name and default namespace.</param>
        /// <param name="outputDirectory">The directory the project is written to. It must not exist or must be empty unless <paramref name="overwrite"/> is set.</param>
        /// <param name="overwrite">When <see langword="true"/>, files already in <paramref name="outputDirectory"/> may be replaced. The caller is expected to have asked the user first.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The full path of the created .csproj file.</returns>
        /// <exception cref="TemplateNotFoundException">Thrown when no matching C# project template exists on the machine.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="outputDirectory"/> already contains files and <paramref name="overwrite"/> is <see langword="false"/>, or when the template engine reports a failure.</exception>
        public async Task<string> CreateProjectAsync(string shortName, string projectName, string outputDirectory, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(shortName, nameof(shortName));
            ArgumentException.ThrowIfNullOrWhiteSpace(projectName, nameof(projectName));
            ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory, nameof(outputDirectory));

            if (TemplateFolder is null)
            {
                throw new TemplateNotFoundException(shortName, TemplatesRoot, TargetFramework);
            }

            if (!overwrite && Directory.Exists(outputDirectory) && Directory.EnumerateFileSystemEntries(outputDirectory).Any())
            {
                throw new InvalidOperationException(
                    $"'{Path.GetFullPath(outputDirectory)}' already contains files. Remove them, choose another output directory, or allow overwriting.");
            }

            var bootstrapper = GetBootstrapper();
            var templates = await bootstrapper.GetTemplatesAsync(cancellationToken).ConfigureAwait(false);

            var template = FindTemplate(templates, shortName);
            if (template is null)
            {
                throw new TemplateNotFoundException(shortName, TemplateFolder, TargetFramework);
            }

            var parameters = new Dictionary<string, string>();
            var frameworkParameter = template.ParameterDefinitions.FirstOrDefault(p => string.Equals(p.Name, FrameworkParameterName, StringComparison.OrdinalIgnoreCase));
            if (frameworkParameter is not null)
            {
                if (frameworkParameter.Choices is not null && !frameworkParameter.Choices.ContainsKey(TargetFramework))
                {
                    throw new InvalidOperationException(
                        $"The '{shortName}' template in '{TemplateFolder}' does not support {TargetFramework}. " +
                        $"Supported frameworks: {string.Join(", ", frameworkParameter.Choices.Keys)}.");
                }
                parameters[FrameworkParameterName] = TargetFramework;
            }

            var result = await bootstrapper.CreateAsync(template, projectName, outputDirectory, parameters, baselineName: null, cancellationToken).ConfigureAwait(false);
            if (result.Status != CreationResultStatus.Success)
            {
                throw new InvalidOperationException($"Creating '{projectName}' from the '{shortName}' template failed ({result.Status}): {result.ErrorMessage}");
            }

            return Path.Combine(Path.GetFullPath(outputDirectory), $"{projectName}.csproj");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _bootstrapper?.Dispose();
            _host?.Dispose();
            _bootstrapper = null;
            _host = null;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Finds the .NET installation root that owns the <c>dotnet</c> host, checking <c>DOTNET_ROOT</c>,
        /// then <c>DOTNET_HOST_PATH</c>, then <c>dotnet</c> on the <c>PATH</c>.
        /// </summary>
        /// <returns>The .NET installation root.</returns>
        /// <exception cref="DirectoryNotFoundException">Thrown when no installation root can be found.</exception>
        public static string FindDotnetRoot()
        {
            var candidates = new List<string>();

            var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
            if (!string.IsNullOrWhiteSpace(dotnetRoot))
            {
                candidates.Add(dotnetRoot);
            }

            var hostPath = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
            if (!string.IsNullOrWhiteSpace(hostPath) && File.Exists(hostPath))
            {
                candidates.Add(Path.GetDirectoryName(ResolveLinkTarget(hostPath)));
            }

            var hostFileName = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
            foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                var candidate = Path.Combine(directory.Trim('"'), hostFileName);
                if (File.Exists(candidate))
                {
                    candidates.Add(Path.GetDirectoryName(ResolveLinkTarget(candidate)));
                }
            }

            var root = candidates.FirstOrDefault(c => Directory.Exists(Path.Combine(c, "templates")));
            if (root is null)
            {
                throw new DirectoryNotFoundException(
                    "Could not find a .NET installation with a 'templates' folder. " +
                    "Set DOTNET_ROOT to the .NET installation directory (the folder that contains 'sdk' and 'templates').");
            }

            return Path.GetFullPath(root);
        }

        /// <summary>
        /// Finds the <c>templates</c> folder of the .NET installation returned by <see cref="FindDotnetRoot"/>.
        /// </summary>
        /// <returns>The full path of the templates folder.</returns>
        public static string FindTemplatesRoot()
        {
            return Path.Combine(FindDotnetRoot(), "templates");
        }

        /// <summary>
        /// Gets the major version of a target framework moniker (for example, <c>10</c> for <c>net10.0</c>).
        /// </summary>
        /// <param name="targetFramework">The target framework moniker.</param>
        /// <returns>The major version.</returns>
        /// <exception cref="ArgumentException">Thrown when the moniker is not in the form <c>netX.Y</c>.</exception>
        public static int GetMajorVersion(string targetFramework)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(targetFramework, nameof(targetFramework));

            if (targetFramework.StartsWith("net", StringComparison.OrdinalIgnoreCase) &&
                Version.TryParse(targetFramework[3..], out var version) &&
                version.Major >= 5)
            {
                return version.Major;
            }

            throw new ArgumentException($"'{targetFramework}' is not a supported target framework. Use the form 'netX.0', for example 'net10.0'.", nameof(targetFramework));
        }

        /// <summary>
        /// Resolves the highest versioned template folder whose major version matches <paramref name="majorVersion"/>.
        /// Stable versions win over prereleases of the same numeric version.
        /// </summary>
        /// <param name="templatesRoot">The folder containing versioned template package folders.</param>
        /// <param name="majorVersion">The major version to match.</param>
        /// <returns>The full path of the matching folder, or <see langword="null"/>.</returns>
        internal static string ResolveTemplateFolder(string templatesRoot, int majorVersion)
        {
            if (!Directory.Exists(templatesRoot))
            {
                return null;
            }

            return Directory.GetDirectories(templatesRoot)
                .Select(d => (Path: d, Version: ParseFolderVersion(Path.GetFileName(d))))
                .Where(d => d.Version.Numeric is not null && d.Version.Numeric.Major == majorVersion)
                .Where(d => Directory.EnumerateFiles(d.Path, "*.nupkg").Any())
                .OrderByDescending(d => d.Version.Numeric)
                .ThenByDescending(d => d.Version.Prerelease is null)
                .ThenByDescending(d => d.Version.Prerelease, StringComparer.OrdinalIgnoreCase)
                .Select(d => d.Path)
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the version of the SDK that supplied a template folder from its common project templates package name.
        /// </summary>
        /// <param name="templateFolder">A versioned template folder, or <see langword="null"/>.</param>
        /// <returns>The SDK version (for example, <c>10.0.401</c>), or <see langword="null"/> when the package isn't there.</returns>
        internal static string ResolveSdkVersion(string templateFolder)
        {
            if (templateFolder is null || !Directory.Exists(templateFolder))
            {
                return null;
            }

            // RWM: The package is named {prefix}{Major}.{Minor}.{SdkVersion}.nupkg, e.g. microsoft.dotnet.common.projecttemplates.10.0.10.0.401.nupkg.
            return Directory.EnumerateFiles(templateFolder, "*.nupkg")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => name.StartsWith(CommonProjectTemplatesPrefix, StringComparison.OrdinalIgnoreCase))
                .Select(name => name[CommonProjectTemplatesPrefix.Length..].Split('.', 3))
                .Where(parts => parts.Length == 3 && int.TryParse(parts[0], out _) && int.TryParse(parts[1], out _))
                .Select(parts => parts[2])
                .FirstOrDefault();
        }

        /// <summary>
        /// Splits a template folder name such as <c>10.0.12</c> or <c>11.0.0-rc.1.26425.128</c> into its numeric
        /// version and optional prerelease label.
        /// </summary>
        /// <param name="folderName">The folder name to parse.</param>
        /// <returns>The numeric version and prerelease label, or <c>(null, null)</c> when the name is not a version.</returns>
        private static (Version Numeric, string Prerelease) ParseFolderVersion(string folderName)
        {
            var dash = folderName.IndexOf('-');
            var numericPart = dash < 0 ? folderName : folderName[..dash];
            var prerelease = dash < 0 ? null : folderName[(dash + 1)..];

            return Version.TryParse(numericPart, out var version) ? (version, prerelease) : (null, null);
        }

        /// <summary>
        /// Follows symbolic links (for example <c>/usr/bin/dotnet</c> on Linux) to the real <c>dotnet</c> host.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <returns>The final link target, or <paramref name="path"/> when it is not a link or cannot be resolved.</returns>
        private static string ResolveLinkTarget(string path)
        {
            try
            {
                var target = new FileInfo(path).ResolveLinkTarget(returnFinalTarget: true);
                return target?.FullName ?? path;
            }
            catch (IOException)
            {
                return path;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Finds the highest-precedence C# project template with the given short name.
        /// </summary>
        /// <param name="templates">The templates the engine found.</param>
        /// <param name="shortName">The template short name, such as <c>classlib</c>.</param>
        /// <returns>The template, or <see langword="null"/> when there is none.</returns>
        private static ITemplateInfo FindTemplate(IEnumerable<ITemplateInfo> templates, string shortName)
        {
            return templates
                .Where(t => t.ShortNameList.Contains(shortName, StringComparer.OrdinalIgnoreCase))
                .Where(t => string.Equals(t.GetLanguage(), CSharpLanguage, StringComparison.OrdinalIgnoreCase))
                .Where(t => string.Equals(t.GetTemplateType(), ProjectTemplateType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.Precedence)
                .FirstOrDefault();
        }

        /// <summary>
        /// Creates the template engine host and bootstrapper on first use, registering the resolved template folder
        /// as the only template package source.
        /// </summary>
        /// <returns>The shared <see cref="Bootstrapper"/> for this host.</returns>
        private Bootstrapper GetBootstrapper()
        {
            if (_bootstrapper is not null)
            {
                return _bootstrapper;
            }

            var version = typeof(SdkTemplateHost).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.0.0";
            var builtIns = new List<(Type InterfaceType, IIdentifiedComponent Instance)>
            {
                (typeof(ITemplatePackageProviderFactory), new FolderTemplatePackageProviderFactory(TemplateFolder))
            };

            _host = new DefaultTemplateEngineHost("dotnet-easyaf", version, defaults: null, builtIns: builtIns);
            _bootstrapper = new Bootstrapper(_host, virtualizeConfiguration: true, loadDefaultComponents: true);
            return _bootstrapper;
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Exposes every <c>.nupkg</c> in a folder as a template package, mirroring the SDK's built-in provider.
        /// </summary>
        private sealed class FolderTemplatePackageProviderFactory : ITemplatePackageProviderFactory
        {

            private readonly string _folder;

            /// <summary>
            /// Initializes a new instance of the <see cref="FolderTemplatePackageProviderFactory"/> class.
            /// </summary>
            /// <param name="folder">The folder whose <c>.nupkg</c> files are exposed as template packages.</param>
            public FolderTemplatePackageProviderFactory(string folder)
            {
                _folder = folder;
            }

            /// <inheritdoc />
            public string DisplayName => "EasyAF SDK templates";

            /// <inheritdoc />
            public Guid Id { get; } = new Guid("6D0B2A0E-3E4E-4C55-9B8C-7E6A6E7E1C51");

            /// <inheritdoc />
            public ITemplatePackageProvider CreateProvider(IEngineEnvironmentSettings settings)
            {
                return new FolderTemplatePackageProvider(this, _folder);
            }

        }

        /// <summary>
        /// The provider created by <see cref="FolderTemplatePackageProviderFactory"/>; lists the folder's packages on demand.
        /// </summary>
        private sealed class FolderTemplatePackageProvider : ITemplatePackageProvider
        {

            private readonly string _folder;

            /// <summary>
            /// Initializes a new instance of the <see cref="FolderTemplatePackageProvider"/> class.
            /// </summary>
            /// <param name="factory">The factory that created this provider.</param>
            /// <param name="folder">The folder whose <c>.nupkg</c> files are exposed as template packages.</param>
            public FolderTemplatePackageProvider(ITemplatePackageProviderFactory factory, string folder)
            {
                Factory = factory;
                _folder = folder;
            }

            /// <inheritdoc />
            public ITemplatePackageProviderFactory Factory { get; }

            /// <inheritdoc />
#pragma warning disable CS0067 // The provider's package list never changes during a run.
            public event Action TemplatePackagesChanged;
#pragma warning restore CS0067

            /// <inheritdoc />
            public Task<IReadOnlyList<ITemplatePackage>> GetAllTemplatePackagesAsync(CancellationToken cancellationToken)
            {
                IReadOnlyList<ITemplatePackage> packages = Directory.EnumerateFiles(_folder, "*.nupkg")
                    .Select(f => (ITemplatePackage)new FolderTemplatePackage(this, f))
                    .ToList();

                return Task.FromResult(packages);
            }

        }

        /// <summary>
        /// A single <c>.nupkg</c> file mounted as a template package.
        /// </summary>
        private sealed class FolderTemplatePackage : ITemplatePackage
        {

            /// <summary>
            /// Initializes a new instance of the <see cref="FolderTemplatePackage"/> class.
            /// </summary>
            /// <param name="provider">The provider that owns this package.</param>
            /// <param name="path">The full path of the <c>.nupkg</c> file.</param>
            public FolderTemplatePackage(ITemplatePackageProvider provider, string path)
            {
                Provider = provider;
                MountPointUri = path;
                LastChangeTime = File.GetLastWriteTimeUtc(path);
            }

            /// <inheritdoc />
            public ITemplatePackageProvider Provider { get; }

            /// <inheritdoc />
            public string MountPointUri { get; }

            /// <inheritdoc />
            public DateTime LastChangeTime { get; }

        }

        #endregion

    }

}
