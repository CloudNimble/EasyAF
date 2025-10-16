using System;
using System.IO;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.EFCoreToEdmx.Models
{

    /// <summary>
    /// Represents the result of converting an EF Core <c>DbContext</c> to an EDMX format.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the name of the <c>DbContext</c> and the generated EDMX content.
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = new EdmxConversionResult("MyDbContext", edmxContent);
    /// await result.WriteToFile("C:\\output");
    /// </code>
    /// </example>
    public class EdmxConversionResult
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name of the <c>DbContext</c> that was converted.
        /// </summary>
        public string DbContextName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the EDMX content generated from the <c>DbContext</c>.
        /// </summary>
        public string EdmxContent { get; set; } = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmxConversionResult"/> class.
        /// </summary>
        /// <param name="dbContextName">The name of the <c>DbContext</c> that was converted.</param>
        /// <param name="edmxContent">The generated EDMX content.</param>
        public EdmxConversionResult(string dbContextName, string edmxContent)
        {
            DbContextName = dbContextName;
            EdmxContent = edmxContent;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Generates the file name for the Entity Data Model (EDM) file.
        /// </summary>
        /// <returns>
        /// A string representing the file name, which consists of the database context name followed by the ".edmx" extension.
        /// </returns>
        public string GetFileName() => $"{DbContextName}.edmx";

        /// <summary>
        /// Writes the EDMX content to a file in the specified directory.
        /// </summary>
        /// <param name="folderPath">The directory path where the EDMX file will be saved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="folderPath"/> is null or empty.</exception>
        public async Task WriteToFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(folderPath));
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            await File.WriteAllTextAsync(Path.Combine(folderPath, GetFileName()), EdmxContent).ConfigureAwait(false);
        }

        #endregion

    }

}
