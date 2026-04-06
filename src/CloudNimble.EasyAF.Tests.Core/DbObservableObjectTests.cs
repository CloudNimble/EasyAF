using CloudNimble.EasyAF.Core;
using CloudNimble.EasyAF.Tests.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.Json;

namespace CloudNimble.EasyAF.Tests.Core
{

    [TestClass]
    public class DbObservableObjectTests
    {

        [TestMethod]
        public void DbObservableObject_Clone_ReturnsCopy()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var person = JsonSerializer.Deserialize<Person>(json);

            person.FirstName.Should().Be("Robert");
            person.LastName.Should().Be("McLaws");

            var result = person.Clone<Person>();
            result.Should().NotBeNull();
            person.Should().BeEquivalentTo(result);
            person.Should().NotBe(result);
        }

        [TestMethod]
        public void DbObservableObject_ToDeltaPayload_ReturnsOnlyChanges()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var person = JsonSerializer.Deserialize<Person>(json);

            person.ShouldTrackChanges.Should().BeFalse();
            person.TrackChanges();
            person.ShouldTrackChanges.Should().BeTrue();
            person.FirstName = "Victoria";
            person.IsChanged.Should().BeTrue();
            person.OriginalValues.Should().HaveCount(1);

            var result = person.ToDeltaPayload();
            result.Should().NotBeEmpty()
                .And.HaveCount(1)
                .And.NotContainKey(nameof(IIdentifiable<Guid>.Id))
                .And.Contain(new KeyValuePair<string, object>(nameof(Person.FirstName), "Victoria"));
        }

        [TestMethod]
        public void DbObservableObject_ToDeltaPayload_Recursive_ReturnsOnlyChanges()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var employee = JsonSerializer.Deserialize<Employee>(json);

            employee.ShouldTrackChanges.Should().BeFalse();
            employee.IsGraphChanged.Should().BeFalse();
            employee.TrackChanges(true);
            employee.ShouldTrackChanges.Should().BeTrue();
            employee.Person.ShouldTrackChanges.Should().BeTrue();
            employee.Person.FirstName = "Ben";

            var result = employee.ToDeltaPayload(true);
            result.Should().NotBeEmpty()
                .And.HaveCount(2)
                .And.ContainKey(nameof(IIdentifiable<Guid>.Id))
                .And.ContainKey(nameof(Person));

            var innerChange = new Dictionary<string, object>(result)[nameof(Person)] as ExpandoObject;
            innerChange.Should().NotBeEmpty()
                .And.HaveCount(1)
                .And.Contain(new KeyValuePair<string, object>(nameof(Person.FirstName), "Ben"));
        }

        [TestMethod]
        public void DbObservableObject_ToDeltaPayload_IIdentifiable_ReturnsOnlyChanges()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var employee = JsonSerializer.Deserialize<Employee>(json);

            employee.ShouldTrackChanges.Should().BeFalse();
            employee.TrackChanges();
            employee.ShouldTrackChanges.Should().BeTrue();
            employee.Title = "Chief Bullshit Officer";
            employee.IsChanged.Should().BeTrue();
            employee.OriginalValues.Should().HaveCount(1);

            var result = employee.ToDeltaPayload(true);
            result.Should().NotBeEmpty()
                .And.HaveCount(2)
                .And.ContainKey(nameof(IIdentifiable<Guid>.Id))
                .And.Contain(new KeyValuePair<string, object>(nameof(Employee.Title), "Chief Bullshit Officer"));
        }


        [TestMethod]
        public void DbObservableObject_AcceptChangesRecursive_HitsAllObjects()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var employee = JsonSerializer.Deserialize<Employee>(json);

            employee.ShouldTrackChanges.Should().BeFalse();
            employee.TrackChanges(true);
            employee.ShouldTrackChanges.Should().BeTrue();
            employee.Title = "Chief Bullshit Officer";
            employee.IsChanged.Should().BeTrue();
            employee.OriginalValues.Should().HaveCount(1);

            var result = employee.ToDeltaPayload(true);
            result.Should().NotBeEmpty()
                .And.HaveCount(2)
                .And.ContainKey(nameof(IIdentifiable<Guid>.Id))
                .And.Contain(new KeyValuePair<string, object>(nameof(Employee.Title), "Chief Bullshit Officer"));
        }

        [TestMethod]
        public void DbObservableObject_IsGraphChanged_Works()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var employee = JsonSerializer.Deserialize<Employee>(json);

            employee.ShouldTrackChanges.Should().BeFalse();
            employee.IsGraphChanged.Should().BeFalse();
            employee.TrackChanges(true);
            employee.ShouldTrackChanges.Should().BeTrue();
            employee.Person.FirstName = "Ben";
            employee.IsChanged.Should().BeFalse();
            employee.Person.IsChanged.Should().BeTrue();
            employee.IsGraphChanged.Should().BeTrue();
        }

    }

}
