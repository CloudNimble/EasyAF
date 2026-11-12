using CloudNimble.EasyAF.Core;
using FluentAssertions;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Restier.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.Restier
{

    /// <summary>
    /// Tests that <see cref="EasyAF_Restier_IModelBuilderExtensions.IgnoreAuditFields{T}(EntitySetConfiguration{T})"/> hides the audit
    /// fields for any value-type creator/updater ID, not just <see cref="int"/> and <see cref="Guid"/>.
    /// </summary>
    [TestClass]
    public class IgnoreAuditFieldsTests
    {

        #region Test Helpers

        /// <summary>
        /// Builds an OData model with a single entity set that ignores audit fields, and returns that entity's property names.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <returns>The names of the properties left in the EDM model.</returns>
        private static string[] ModelPropertyNames<T>() where T : EasyObservableObject
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<T>("Items").IgnoreAuditFields();
            var entityType = builder.GetEdmModel().SchemaElements.OfType<IEdmEntityType>().Single(c => c.Name == typeof(T).Name);
            return [.. entityType.Properties().Select(c => c.Name)];
        }

        #endregion

        #region IgnoreAuditFields Tests

        [TestMethod]
        public void IgnoreAuditFields_WithLongIds_ShouldRemoveCreatorAndUpdater()
        {
            var properties = ModelPropertyNames<LongTrackedEntity>();

            properties.Should().NotContain(new[] { "CreatedById", "UpdatedById", "DateCreated", "DateUpdated" });
            properties.Should().Contain(new[] { "Id", "DisplayName" });
        }

        [TestMethod]
        public void IgnoreAuditFields_WithGuidIds_ShouldRemoveCreatorAndUpdater()
        {
            var properties = ModelPropertyNames<GuidTrackedEntity>();

            properties.Should().NotContain(new[] { "CreatedById", "UpdatedById", "DateCreated", "DateUpdated" });
            properties.Should().Contain(new[] { "Id", "DisplayName" });
        }

        #endregion

        #region Test Types

        /// <summary>
        /// An entity with <see cref="long"/> creator and updater IDs.
        /// </summary>
        public class LongTrackedEntity : EasyObservableObject, IIdentifiable<long>, ICreatorTrackable<long>, IUpdaterTrackable<long>, ICreatedAuditable, IUpdatedAuditable
        {
            public long Id { get; set; }

            public string DisplayName { get; set; }

            public long CreatedById { get; set; }

            public long? UpdatedById { get; set; }

            public DateTimeOffset DateCreated { get; set; }

            public DateTimeOffset? DateUpdated { get; set; }
        }

        /// <summary>
        /// An entity with <see cref="Guid"/> creator and updater IDs.
        /// </summary>
        public class GuidTrackedEntity : EasyObservableObject, IIdentifiable<Guid>, ICreatorTrackable<Guid>, IUpdaterTrackable<Guid>, ICreatedAuditable, IUpdatedAuditable
        {
            public Guid Id { get; set; }

            public string DisplayName { get; set; }

            public Guid CreatedById { get; set; }

            public Guid? UpdatedById { get; set; }

            public DateTimeOffset DateCreated { get; set; }

            public DateTimeOffset? DateUpdated { get; set; }
        }

        #endregion

    }

}
