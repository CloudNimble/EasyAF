using CloudNimble.EasyAF.Core;
using CloudNimble.EasyAF.Tests.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Tests.Core.Extensions
{

    [TestClass]
    public class JsonSerializerOptionsExtensionsTests
    {

        #region Happy Path

        [TestMethod]
        public void IgnoreAuditFields_ReturnsSameInstance()
        {
            var options = new JsonSerializerOptions();

            options.IgnoreAuditFields().Should().BeSameAs(options);
        }

        [TestMethod]
        public void IgnoreAuditFields_Serialize_OmitsAuditFields()
        {
            var options = new JsonSerializerOptions().IgnoreAuditFields();

            var result = JsonSerializer.Serialize(new AuditableConcert { DateCreated = DateTimeOffset.UtcNow }, options);

            result.Should().NotContain(nameof(AuditableConcert.DateCreated))
                .And.Contain(nameof(AuditableConcert.Id))
                .And.NotContain(nameof(DbObservableObject.IsChanged))
                .And.NotContain(nameof(DbObservableObject.OriginalValues));
        }

        [TestMethod]
        public void IgnoreAuditFields_Deserialize_ReadsAuditFields()
        {
            var options = new JsonSerializerOptions().IgnoreAuditFields();
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//AuditableConcert.json");

            var result = JsonSerializer.Deserialize<AuditableConcert>(json, options);

            result.DateCreated.Should().NotBe(DateTimeOffset.MinValue);
        }

        [TestMethod]
        public void IgnoreAuditFields_Serialize_OmitsAuditFieldsOnNestedEntities()
        {
            var options = new JsonSerializerOptions().IgnoreAuditFields();
            var tour = new AuditableTour
            {
                DateCreated = DateTimeOffset.UtcNow,
                Name = "World Tour",
                Headliner = new AuditableConcert { DateCreated = DateTimeOffset.UtcNow },
            };

            var result = JsonSerializer.Serialize(tour, options);

            result.Should().NotContain(nameof(AuditableTour.DateCreated))
                .And.Contain(nameof(AuditableTour.Headliner));
        }

        [TestMethod]
        public void IgnoreAuditFields_Serialize_HonorsNamingPolicy()
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }.IgnoreAuditFields();

            var result = JsonSerializer.Serialize(new AuditableConcert { DateCreated = DateTimeOffset.UtcNow }, options);

            result.Should().NotContainEquivalentOf("dateCreated")
                .And.Contain("\"id\"");
        }

        [TestMethod]
        public void IgnoreAuditFields_Serialize_HonorsWhenWritingDefault()
        {
            var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault }.IgnoreAuditFields();

            var result = JsonSerializer.Serialize(new AuditableConcert(), options);

            result.Should().Be("{}");
        }

        [TestMethod]
        public void IgnoreAuditFields_Serialize_WritesNullsByDefault()
        {
            var options = new JsonSerializerOptions().IgnoreAuditFields();

            var result = JsonSerializer.Serialize(new AuditableConcert(), options);

            result.Should().Contain(nameof(AuditableConcert.Attendees));
        }

        [TestMethod]
        public void IgnoreAuditFields_KeepsExistingResolver()
        {
            var options = new JsonSerializerOptions { TypeInfoResolver = TestJsonContext.Default }.IgnoreAuditFields();

            var result = JsonSerializer.Serialize(new AuditableConcert { DateCreated = DateTimeOffset.UtcNow }, options);

            result.Should().NotContain(nameof(AuditableConcert.DateCreated))
                .And.Contain(nameof(AuditableConcert.Id));
        }

        [TestMethod]
        public void IgnoreAuditFields_DoesNotAffectNonEntityTypes()
        {
            var options = new JsonSerializerOptions().IgnoreAuditFields();

            var result = JsonSerializer.Serialize(new PlainAuditRecord { DateCreated = DateTimeOffset.UtcNow }, options);

            result.Should().Contain(nameof(PlainAuditRecord.DateCreated));
        }

        #endregion

        #region Unhappy Path

        [TestMethod]
        public void IgnoreAuditFields_NullOptions_Throws()
        {
            JsonSerializerOptions options = null;

            var act = () => options.IgnoreAuditFields();

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void IgnoreAuditFields_ReadOnlyOptions_Throws()
        {
            var options = new JsonSerializerOptions();
            options.MakeReadOnly(populateMissingResolver: true);

            var act = () => options.IgnoreAuditFields();

            act.Should().Throw<InvalidOperationException>();
        }

        #endregion

        #region Test Types

        /// <summary>
        /// Not a <see cref="DbObservableObject"/>, so its audit-named properties must be left alone.
        /// </summary>
        public class PlainAuditRecord
        {
            public DateTimeOffset DateCreated { get; set; }
        }

        #endregion

    }

    [JsonSerializable(typeof(AuditableConcert))]
    internal partial class TestJsonContext : JsonSerializerContext;

}
