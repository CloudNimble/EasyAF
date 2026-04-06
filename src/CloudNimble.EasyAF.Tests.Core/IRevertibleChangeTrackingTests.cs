using CloudNimble.EasyAF.Tests.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CloudNimble.EasyAF.Tests.Core
{

    [TestClass]
    public class IRevertibleChangeTrackingTests
    {

        [TestMethod]
        public void Revertible_HasCorrectDefaults()
        {
            var person = new Person();

            person.ShouldTrackChanges.Should().BeFalse();
            person.FirstName.Should().BeNullOrWhiteSpace();
            person.LastName.Should().BeNullOrWhiteSpace();
            person.IsChanged.Should().BeFalse();
            person.OriginalValues.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public void Revertible_NoTracking_RaisesPropertyChanged_NoOriginalValues()
        {
            var person = new Person();
            using var monitor = person.Monitor();

            person.ShouldTrackChanges.Should().BeFalse();
            person.FirstName.Should().BeNullOrWhiteSpace();
            person.LastName.Should().BeNullOrWhiteSpace();
            person.IsChanged.Should().BeFalse();
            person.OriginalValues.Should().NotBeNull().And.BeEmpty();

            person.FirstName = "Robert";

            person.IsChanged.Should().BeFalse();
            monitor.OccurredEvents.Where(c => c.EventName == "PropertyChanged").Should().HaveCount(1);
            monitor.Should().RaisePropertyChangeFor(c => c.FirstName);
            monitor.Should().NotRaisePropertyChangeFor(c => c.LastName);
            person.OriginalValues.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public void Revertible_Tracking_RaisesPropertyChanged_HasOriginalValues()
        {
            var person = new Person();
            using var monitor = person.Monitor();

            person.ShouldTrackChanges.Should().BeFalse();
            person.FirstName.Should().BeNullOrWhiteSpace();
            person.LastName.Should().BeNullOrWhiteSpace();
            person.OriginalValues.Should().BeEmpty();

            person.ShouldTrackChanges = true;
            person.FirstName = "Robert";

            person.IsChanged.Should().BeTrue();
            monitor.OccurredEvents.Where(c => c.EventName == "PropertyChanged").Should().HaveCount(1);
            monitor.Should().RaisePropertyChangeFor(c => c.FirstName);
            monitor.Should().NotRaisePropertyChangeFor(c => c.LastName);
            person.OriginalValues.Should().NotBeNull().And.HaveCount(1);
        }

        [TestMethod]
        public void Revertible_Tracking_AcceptChanges_ResetsChangeTracking()
        {
            var person = new Person();
            using var monitor = person.Monitor();

            person.ShouldTrackChanges.Should().BeFalse();
            person.FirstName.Should().BeNullOrWhiteSpace();
            person.LastName.Should().BeNullOrWhiteSpace();
            person.OriginalValues.Should().BeEmpty();

            person.ShouldTrackChanges = true;
            person.FirstName = "Robert";

            person.IsChanged.Should().BeTrue();
            monitor.OccurredEvents.Where(c => c.EventName == "PropertyChanged").Should().HaveCount(1);
            monitor.Should().RaisePropertyChangeFor(c => c.FirstName);
            monitor.Should().NotRaisePropertyChangeFor(c => c.LastName);
            person.OriginalValues.Should().NotBeNull().And.HaveCount(1);

            person.AcceptChanges();
            person.IsChanged.Should().BeFalse();
            person.OriginalValues.Should().NotBeNull().And.HaveCount(0);
        }

        [TestMethod]
        public void Revertible_Tracking_RejectChanges_ResetsObject()
        {
            var person = new Person();
            using var monitor = person.Monitor();

            person.ShouldTrackChanges.Should().BeFalse();
            person.FirstName.Should().BeNullOrWhiteSpace();
            person.LastName.Should().BeNullOrWhiteSpace();
            person.OriginalValues.Should().BeEmpty();

            person.ShouldTrackChanges = true;
            person.FirstName = "Robert";

            person.IsChanged.Should().BeTrue();
            monitor.OccurredEvents.Where(c => c.EventName == "PropertyChanged").Should().HaveCount(1);
            monitor.Should().RaisePropertyChangeFor(c => c.FirstName);
            monitor.Should().NotRaisePropertyChangeFor(c => c.LastName);
            person.OriginalValues.Should().NotBeNull().And.HaveCount(1);

            person.RejectChanges();
            person.FirstName.Should().BeNullOrWhiteSpace();
            person.IsChanged.Should().BeFalse();
            person.OriginalValues.Should().NotBeNull().And.HaveCount(0);
        }

        [TestMethod]
        public void Revertible_Deserialized_NoTracking_RaisesPropertyChanged_NoOriginalValues()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var person = JsonSerializer.Deserialize<Person>(json);

            person.ShouldTrackChanges.Should().BeFalse();
            person.FirstName.Should().NotBeNullOrWhiteSpace();
            person.LastName.Should().NotBeNullOrWhiteSpace();
            person.OriginalValues.Should().NotBeNull().And.BeEmpty();
        }

        [TestMethod]
        public void Revertible_Deserialized_Tracking_SameValue_DoesntFireChanges()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var person = JsonSerializer.Deserialize<Person>(json);
            using var monitor = person.Monitor();

            person.FirstName.Should().NotBeNullOrWhiteSpace();
            person.LastName.Should().NotBeNullOrWhiteSpace();
            person.OriginalValues.Should().NotBeNull().And.BeEmpty();

            person.ShouldTrackChanges = true;
            person.FirstName = "Robert";

            person.IsChanged.Should().BeFalse();
            monitor.OccurredEvents.Where(c => c.EventName == "PropertyChanged").Should().HaveCount(0);
            monitor.Should().NotRaisePropertyChangeFor(c => c.FirstName);
            monitor.Should().NotRaisePropertyChangeFor(c => c.LastName);
            person.OriginalValues.Should().HaveCount(0);
        }

        [TestMethod]
        public void Revertible_Deserialized_Tracking_RaisesPropertyChanged_HasOriginalValues()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var person = JsonSerializer.Deserialize<Person>(json);
            using var monitor = person.Monitor();

            person.FirstName.Should().NotBeNullOrWhiteSpace();
            person.LastName.Should().NotBeNullOrWhiteSpace();
            person.OriginalValues.Should().NotBeNull().And.BeEmpty();

            person.ShouldTrackChanges = true;
            person.FirstName = "Victoria";

            person.IsChanged.Should().BeTrue();
            monitor.OccurredEvents.Where(c => c.EventName == "PropertyChanged").Should().HaveCount(1);
            monitor.Should().RaisePropertyChangeFor(c => c.FirstName);
            monitor.Should().NotRaisePropertyChangeFor(c => c.LastName);
            person.OriginalValues.Should().HaveCount(1);
        }

        [TestMethod]
        public void Revertible_Deserialized_Tracking_AcceptChanges_ResetsChangeTracking()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var person = JsonSerializer.Deserialize<Person>(json);
            using var monitor = person.Monitor();

            person.FirstName.Should().NotBeNullOrWhiteSpace();
            person.LastName.Should().NotBeNullOrWhiteSpace();
            person.OriginalValues.Should().NotBeNull().And.BeEmpty();

            person.ShouldTrackChanges = true;
            person.FirstName = "Victoria";

            person.IsChanged.Should().BeTrue();
            monitor.OccurredEvents.Where(c => c.EventName == "PropertyChanged").Should().HaveCount(1);
            monitor.Should().RaisePropertyChangeFor(c => c.FirstName);
            monitor.Should().NotRaisePropertyChangeFor(c => c.LastName);
            person.OriginalValues.Should().HaveCount(1);

            person.AcceptChanges();
            person.IsChanged.Should().BeFalse();
            person.OriginalValues.Should().NotBeNull().And.HaveCount(0);
        }

        [TestMethod]
        public void Revertible_Deserialized_Tracking_RejectChanges_ResetsObject()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var person = JsonSerializer.Deserialize<Person>(json);
            using var monitor = person.Monitor();

            person.FirstName.Should().NotBeNullOrWhiteSpace();
            person.LastName.Should().NotBeNullOrWhiteSpace();
            person.OriginalValues.Should().NotBeNull().And.BeEmpty();

            person.ShouldTrackChanges = true;
            person.FirstName = "Victoria";

            person.IsChanged.Should().BeTrue();
            monitor.OccurredEvents.Where(c => c.EventName == "PropertyChanged").Should().HaveCount(1);
            monitor.Should().RaisePropertyChangeFor(c => c.FirstName);
            monitor.Should().NotRaisePropertyChangeFor(c => c.LastName);
            person.OriginalValues.Should().HaveCount(1);

            person.RejectChanges();
            person.FirstName.Should().Be("Robert");
            person.IsChanged.Should().BeFalse();
            person.OriginalValues.Should().NotBeNull().And.HaveCount(0);
        }

    }

}
