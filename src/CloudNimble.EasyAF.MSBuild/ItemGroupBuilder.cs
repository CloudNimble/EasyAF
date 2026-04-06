using CloudNimble.EasyAF.Core;
using Microsoft.Build.Construction;
using System;
using System.Linq;

namespace CloudNimble.EasyAF.MSBuild
{

    /// <summary>
    /// Builder class for configuring MSBuild ItemGroups in a fluent manner.
    /// </summary>
    /// <remarks>
    /// This class provides a fluent API for adding items to MSBuild ItemGroups,
    /// making it easier to construct complex project structures programmatically.
    /// </remarks>
    public class ItemGroupBuilder
    {

        #region Fields

        /// <summary>
        /// The ItemGroup being configured.
        /// </summary>
        private readonly ProjectItemGroupElement _itemGroup;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemGroupBuilder"/> class.
        /// </summary>
        /// <param name="itemGroup">The ItemGroup to configure.</param>
        /// <exception cref="ArgumentNullException">Thrown when itemGroup is null.</exception>
        internal ItemGroupBuilder(ProjectItemGroupElement itemGroup)
        {
            Ensure.ArgumentNotNull(itemGroup, nameof(itemGroup));
            _itemGroup = itemGroup;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a PackageReference item to the ItemGroup.
        /// </summary>
        /// <param name="packageId">The package ID.</param>
        /// <param name="version">The package version.</param>
        /// <returns>An ItemBuilder for further configuration of the PackageReference.</returns>
        /// <exception cref="ArgumentException">Thrown when packageId or version is null or whitespace.</exception>
        public ItemBuilder AddPackageReference(string packageId, string version)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(packageId, nameof(packageId));
            Ensure.ArgumentNotNullOrWhiteSpace(version, nameof(version));

            var item = _itemGroup.AddItem("PackageReference", packageId);
            item.AddMetadata("Version", version);

            return new ItemBuilder(item);
        }

        /// <summary>
        /// Adds an AdditionalFiles item to the ItemGroup.
        /// </summary>
        /// <param name="include">The file pattern to include.</param>
        /// <returns>An ItemBuilder for further configuration of the AdditionalFiles item.</returns>
        /// <exception cref="ArgumentException">Thrown when include is null or whitespace.</exception>
        public ItemBuilder AddAdditionalFiles(string include)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(include, nameof(include));

            var item = _itemGroup.AddItem("AdditionalFiles", include);
            return new ItemBuilder(item);
        }

        /// <summary>
        /// Adds a generic item to the ItemGroup.
        /// </summary>
        /// <param name="itemType">The type of the item.</param>
        /// <param name="include">The include value for the item.</param>
        /// <returns>An ItemBuilder for further configuration of the item.</returns>
        /// <exception cref="ArgumentException">Thrown when itemType or include is null or whitespace.</exception>
        public ItemBuilder AddItem(string itemType, string include)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(itemType, nameof(itemType));
            Ensure.ArgumentNotNullOrWhiteSpace(include, nameof(include));

            var item = _itemGroup.AddItem(itemType, include);
            return new ItemBuilder(item);
        }

        #endregion

    }

}
