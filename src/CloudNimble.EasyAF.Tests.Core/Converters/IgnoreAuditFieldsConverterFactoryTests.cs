using CloudNimble.EasyAF.Core;
using CloudNimble.EasyAF.Core.Converters;
using CloudNimble.EasyAF.Tests.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

// RWM: These tests cover the obsolete converter until it is removed.
#pragma warning disable CS0618

namespace CloudNimble.EasyAF.Tests.Core.Converters
{

    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class IgnoreAuditFieldsConverterFactoryTests
    {

        [TestMethod]
        public void AuditableConcert_Deserialize_ShouldHavePropertiesSet()
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Converters =
                {
                    new IgnoreAuditFieldsJsonConverterFactory()
                }
            };

            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//AuditableConcert.json");
            var auditableConcert = JsonSerializer.Deserialize<AuditableConcert>(json, jsonSerializerOptions);
            auditableConcert.Should().NotBeNull();
            auditableConcert.DateCreated.Should().NotBe(DateTimeOffset.MinValue);
        }

        [TestMethod]
        public void AuditableConcert_Serialize_ShouldNotHaveDateCreated_AndNotHaveNulls()
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Converters =
                {
                    new IgnoreAuditFieldsJsonConverterFactory()
                }
            };

            var auditableConcert = new AuditableConcert
            {
                DateCreated = DateTimeOffset.UtcNow,
                Organizer = new Person
                {
                    FirstName = "James",
                    LastName = "Caldwell"
                }
            };

            var result = JsonSerializer.Serialize(auditableConcert, jsonSerializerOptions);
            result.Should().NotBeNullOrWhiteSpace()
                .And.NotContain("DateCreated")
                .And.NotContain("Attendees")
                .And.NotContain(nameof(DbObservableObject.IsChanged))
                .And.NotContain(nameof(DbObservableObject.IsGraphChanged))
                .And.NotContain(nameof(DbObservableObject.OriginalValues));
        }

        [TestMethod]
        public void AuditableConcert_Serialize_ShouldNotHaveDateCreated_AndHaveNulls()
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new IgnoreAuditFieldsJsonConverterFactory()
                }
            };

            var auditableConcert = new AuditableConcert
            {
                DateCreated = DateTimeOffset.UtcNow,
                Organizer = new Person
                {
                    FirstName = "James",
                    LastName = "Caldwell"
                }
            };

            var result = JsonSerializer.Serialize(auditableConcert, jsonSerializerOptions);
            result.Should().NotBeNullOrWhiteSpace()
                .And.NotContain("DateCreated")
                .And.Contain("Attendees")
                .And.NotContain(nameof(DbObservableObject.IsChanged))
                .And.NotContain(nameof(DbObservableObject.IsGraphChanged))
                .And.NotContain(nameof(DbObservableObject.OriginalValues));

        }

        [TestMethod]
        public void Factory_Serialize_OmitsAuditFieldsOnNestedEntities()
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new IgnoreAuditFieldsJsonConverterFactory()
                }
            };

            var tour = new AuditableTour
            {
                DateCreated = DateTimeOffset.UtcNow,
                Headliner = new AuditableConcert { DateCreated = DateTimeOffset.UtcNow },
            };

            var result = JsonSerializer.Serialize(tour, jsonSerializerOptions);
            result.Should().NotContain("DateCreated")
                .And.Contain(nameof(AuditableTour.Headliner));
        }

        [TestMethod]
        public void Factory_Serialize_HonorsNamingPolicy()
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new IgnoreAuditFieldsJsonConverterFactory()
                }
            };

            var result = JsonSerializer.Serialize(new AuditableConcert { DateCreated = DateTimeOffset.UtcNow }, jsonSerializerOptions);
            result.Should().Contain("\"id\"")
                .And.NotContainEquivalentOf("dateCreated");
        }

        [TestMethod]
        public void Factory_Serialize_HonorsWhenWritingDefault()
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Converters =
                {
                    new IgnoreAuditFieldsJsonConverterFactory()
                }
            };

            var result = JsonSerializer.Serialize(new AuditableConcert(), jsonSerializerOptions);
            result.Should().Be("{}", because: "5.0 no longer writes Guid.Empty and other default values under WhenWritingDefault");
        }

        [TestMethod]
        public void Factory_Serialize_WhenWritingNull_RestoresLegacyDefaultValues()
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new IgnoreAuditFieldsJsonConverterFactory()
                }
            };

            var result = JsonSerializer.Serialize(new AuditableConcert(), jsonSerializerOptions);
            result.Should().Be($"{{\"Id\":\"{Guid.Empty}\"}}");
        }

    }

}
