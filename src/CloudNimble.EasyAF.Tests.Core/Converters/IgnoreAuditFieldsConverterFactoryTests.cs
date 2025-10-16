using CloudNimble.EasyAF.Core;
using CloudNimble.EasyAF.Core.Converters;
using CloudNimble.EasyAF.Tests.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    }

}
