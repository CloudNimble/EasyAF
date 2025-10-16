using CloudNimble.EasyAF.Core;
using Microsoft.Build.Construction;
using System;

namespace CloudNimble.EasyAF.MSBuild
{
    /// <summary>
    /// Builder class for configuring individual MSBuild items in a fluent manner.
    /// </summary>
    /// <remarks>
    /// This class provides a fluent API for adding metadata to MSBuild items.
    /// </remarks>
    public class ItemBuilder
    {

        #region Fields

        /// <summary>
        /// The item being configured.
        /// </summary>
        private readonly ProjectItemElement _item;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemBuilder"/> class.
        /// </summary>
        /// <param name="item">The item to configure.</param>
        /// <exception cref="ArgumentNullException">Thrown when item is null.</exception>
        internal ItemBuilder(ProjectItemElement item)
        {
            Ensure.ArgumentNotNull(item, nameof(item));
            _item = item;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds metadata to the item.
        /// </summary>
        /// <param name="name">The metadata name.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when name or value is null or whitespace.</exception>
        public ItemBuilder AddMetadata(string name, string value)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            Ensure.ArgumentNotNullOrWhiteSpace(value, nameof(value));

            _item.AddMetadata(name, value);
            return this;
        }

        /// <summary>
        /// Sets the PrivateAssets metadata for the item (commonly used with PackageReference).
        /// </summary>
        /// <param name="value">The PrivateAssets value (e.g., "all", "runtime", "compile").</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when value is null or whitespace.</exception>
        public ItemBuilder SetPrivateAssets(string value)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(value, nameof(value));

            return AddMetadata("PrivateAssets", value);
        }

        /// <summary>
        /// Sets the Link metadata for the item (commonly used with AdditionalFiles).
        /// </summary>
        /// <param name="value">The Link value.</param>
        /// <returns>The current instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when value is null or whitespace.</exception>
        public ItemBuilder SetLink(string value)
        {
            Ensure.ArgumentNotNullOrWhiteSpace(value, nameof(value));

            return AddMetadata("Link", value);
        }

        /// <summary>
        /// Sets the Visible metadata for the item.
        /// </summary>
        /// <param name="visible">Whether the item should be visible.</param>
        /// <returns>The current instance for method chaining.</returns>
        public ItemBuilder SetVisible(bool visible)
        {
            return AddMetadata("Visible", visible.ToString().ToLowerInvariant());
        }

        #endregion

    }

}
