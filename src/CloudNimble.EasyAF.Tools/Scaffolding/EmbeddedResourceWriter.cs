using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tools.Scaffolding
{

    /// <summary>
    /// Renders text files embedded in an assembly, replacing <c>{{Token}}</c> placeholders, and writes them to disk.
    /// </summary>
    /// <remarks>
    /// Resources are expected to use path-preserving logical names (for example <c>Resources/Api/Program.cs</c>) so that
    /// folder names containing dots are not confused with nested folders. Tokens are replaced in a single pass, so a token
    /// value that itself looks like a token is written literally. A placeholder with no matching token is an error rather
    /// than something to ship. Line endings are normalized to <see cref="Environment.NewLine"/>.
    /// </remarks>
    public partial class EmbeddedResourceWriter
    {

        #region Fields

        private const string DefaultRootFolder = "Resources";

        private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

        private readonly Dictionary<string, string> _manifestNames;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the assembly the resources are read from.
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// Gets the names of the available resources, relative to <see cref="RootFolder"/>, with forward slashes, sorted.
        /// </summary>
        public IReadOnlyList<string> ResourceNames { get; }

        /// <summary>
        /// Gets the logical folder that resource names are relative to (for example <c>Resources</c>).
        /// </summary>
        public string RootFolder { get; }

        /// <summary>
        /// Gets the tokens that replace <c>{{Name}}</c> placeholders.
        /// </summary>
        public IReadOnlyDictionary<string, string> Tokens { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceWriter"/> class that reads the EasyAF Tools
        /// assembly's <c>Resources/</c> folder.
        /// </summary>
        /// <param name="tokens">The tokens that replace <c>{{Name}}</c> placeholders.</param>
        public EmbeddedResourceWriter(IReadOnlyDictionary<string, string> tokens)
            : this(tokens, typeof(EmbeddedResourceWriter).Assembly, DefaultRootFolder)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedResourceWriter"/> class.
        /// </summary>
        /// <param name="tokens">The tokens that replace <c>{{Name}}</c> placeholders.</param>
        /// <param name="assembly">The assembly the resources are embedded in.</param>
        /// <param name="rootFolder">The logical folder resource names are relative to.</param>
        public EmbeddedResourceWriter(IReadOnlyDictionary<string, string> tokens, Assembly assembly, string rootFolder)
        {
            ArgumentNullException.ThrowIfNull(tokens, nameof(tokens));
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));
            ArgumentException.ThrowIfNullOrWhiteSpace(rootFolder, nameof(rootFolder));

            Tokens = new Dictionary<string, string>(tokens);
            Assembly = assembly;
            RootFolder = rootFolder.Trim().Trim('/', '\\');

            var prefix = RootFolder + "/";
            _manifestNames = assembly.GetManifestResourceNames()
                .Select(name => (Manifest: name, Normalized: NormalizeName(name)))
                .Where(n => n.Normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(n => n.Normalized[prefix.Length..], n => n.Manifest, StringComparer.OrdinalIgnoreCase);

            ResourceNames = [.. _manifestNames.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase)];
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Renders a resource with its tokens replaced.
        /// </summary>
        /// <param name="resourceName">The resource name relative to <see cref="RootFolder"/>, for example <c>Api/Program.cs</c>.</param>
        /// <returns>The rendered text.</returns>
        /// <exception cref="FileNotFoundException">Thrown when no resource with that name exists.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the resource contains a placeholder with no matching token.</exception>
        public string Render(string resourceName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(resourceName, nameof(resourceName));

            var key = NormalizeName(resourceName).Trim('/');
            if (!_manifestNames.TryGetValue(key, out var manifestName))
            {
                throw new FileNotFoundException($"The embedded resource '{RootFolder}/{key}' was not found in {Assembly.GetName().Name}.", key);
            }

            string text;
            using (var stream = Assembly.GetManifestResourceStream(manifestName))
            using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                text = reader.ReadToEnd();
            }

            var rendered = TokenPattern().Replace(text, match =>
            {
                var tokenName = match.Groups[1].Value;
                return Tokens.TryGetValue(tokenName, out var value)
                    ? value
                    : throw new InvalidOperationException($"The placeholder {match.Value} in '{RootFolder}/{key}' has no matching token.");
            });

            return LineEndingPattern().Replace(rendered, Environment.NewLine);
        }

        /// <summary>
        /// Renders a resource and writes it to disk as UTF-8 without a byte order mark, creating parent directories and
        /// replacing any existing file.
        /// </summary>
        /// <param name="resourceName">The resource name relative to <see cref="RootFolder"/>.</param>
        /// <param name="destinationPath">The file to write.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The full path of the written file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when no resource with that name exists.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the resource contains a placeholder with no matching token. Nothing is written.</exception>
        public async Task<string> WriteAsync(string resourceName, string destinationPath, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath, nameof(destinationPath));

            var content = Render(resourceName);
            var fullPath = Path.GetFullPath(destinationPath);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            await File.WriteAllTextAsync(fullPath, content, Utf8NoBom, cancellationToken).ConfigureAwait(false);

            return fullPath;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Normalizes a resource name to forward slashes.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <returns>The name with backslashes replaced by forward slashes.</returns>
        private static string NormalizeName(string name)
        {
            return name.Replace('\\', '/');
        }

        /// <summary>
        /// Matches a <c>{{Name}}</c> placeholder, capturing the name.
        /// </summary>
        [GeneratedRegex(@"\{\{([A-Za-z][A-Za-z0-9]*)\}\}")]
        private static partial Regex TokenPattern();

        /// <summary>
        /// Matches any line ending: CRLF, LF, or a lone CR.
        /// </summary>
        [GeneratedRegex(@"\r\n|\n|\r")]
        private static partial Regex LineEndingPattern();

        #endregion

    }

}
