using CloudNimble.EasyAF.Tests.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CloudNimble.EasyAF.Tests.Core
{

    [TestClass]
    public class IEnumerableExtensionsTests
    {

        #region ChangedCount()

        [TestMethod]
        public void ChangedCount_ShallowCheck_Returns0()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var list = new List<Person>
            {
                JsonSerializer.Deserialize<Person>(json),
                JsonSerializer.Deserialize<Person>(json),
                JsonSerializer.Deserialize<Person>(json),
            };

            list[0].FirstName = "James";
            list.ChangedCount().Should().Be(0);
        }

        [TestMethod]
        public void ChangedCount_ShallowCheck_Returns1()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var list = new List<Person>
            {
                JsonSerializer.Deserialize<Person>(json),
                JsonSerializer.Deserialize<Person>(json),
                JsonSerializer.Deserialize<Person>(json),
            };

            list = list.ToTrackedList();
            list[0].FirstName = "James";
            list.ChangedCount().Should().Be(1);
        }

        [TestMethod]
        public void ChangedCount_ShallowCheck_Returns2()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var list = new List<Person>
            {
                JsonSerializer.Deserialize<Person>(json),
                JsonSerializer.Deserialize<Person>(json),
                JsonSerializer.Deserialize<Person>(json),
            };

            list = list.ToTrackedList();
            list[0].FirstName = "James";
            list[2].FirstName = "Amy";
            list.ChangedCount().Should().Be(2);
        }

        #endregion

        #region ContentsAreChanged()

        [TestMethod]
        public void ContentsAreChanged_ShallowCheck_NotTracking_ReturnsFalse()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var list = new List<Person>
            {
                JsonSerializer.Deserialize<Person>(json),
                JsonSerializer.Deserialize<Person>(json),
                JsonSerializer.Deserialize<Person>(json),
            };

            //list = list.ToTrackedList(); // RWM: No tracking here.
            list[0].FirstName = "James";
            list.ContentsAreChanged().Should().BeFalse();
        }

        [TestMethod]
        public void ContentsAreChanged_ShallowCheck_ReturnsTrue()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var list = new List<Person>
            {
                JsonSerializer.Deserialize<Person>(json),
                JsonSerializer.Deserialize<Person>(json),
                JsonSerializer.Deserialize<Person>(json),
            };

            list = list.ToTrackedList();
            list[0].FirstName = "James";
            list.ContentsAreChanged().Should().BeTrue();
        }

        [TestMethod]
        public void ContentsAreChanged_DeepCheck_NotDeepTracking_ReturnsFalse()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var list = new List<Employee>
            {
                JsonSerializer.Deserialize<Employee>(json),
                JsonSerializer.Deserialize<Employee>(json),
                JsonSerializer.Deserialize<Employee>(json),
            };

            list = list.ToTrackedList();
            list[1].Person.FirstName = "James";
            list.ContentsAreChanged(true).Should().BeFalse();
        }

        [TestMethod]
        public void ContentsAreChanged_DeepCheck_NotTracking_ReturnsFalse()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var list = new List<Employee>
            {
                JsonSerializer.Deserialize<Employee>(json),
                JsonSerializer.Deserialize<Employee>(json),
                JsonSerializer.Deserialize<Employee>(json),
            };

            //list = list.ToTrackedList(); // RWM: No tracking here.
            list[1].Person.FirstName = "James";
            list.ContentsAreChanged(true).Should().BeFalse();
        }

        [TestMethod]
        public void ContentsAreChanged_DeepCheck_ReturnsTrue()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var list = new List<Employee>
            {
                JsonSerializer.Deserialize<Employee>(json),
                JsonSerializer.Deserialize<Employee>(json),
                JsonSerializer.Deserialize<Employee>(json),
            };

            list = list.ToTrackedList(true);
            list[1].Person.FirstName = "James";
            list.ContentsAreChanged(true).Should().BeTrue();
        }

        [TestMethod]
        public void ContentsAreChanged_WhereClause_FiltersProperly_ReturnsFalse()
        {
            var empJson = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var deptJson = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Department.json");

            var departmentsList = new List<Department>
            {
                JsonSerializer.Deserialize<Department>(deptJson),
                new Department { Id = Guid.NewGuid(), DisplayName = "Sales" },
                new Department { Id = Guid.NewGuid(), DisplayName = "CustomerSuccess" },
            };

            var employeesList = new List<Employee>
            {
                JsonSerializer.Deserialize<Employee>(empJson),
                JsonSerializer.Deserialize<Employee>(empJson),
            }.ToTrackedList();

            employeesList[0].Id = Guid.NewGuid();
            employeesList[0].Title = "Chief Idiot";

            // RWM: Run a query that will never return a positive result.
            employeesList.ContentsAreChanged(c => c.Id == Guid.NewGuid()).Should().BeFalse();
        }

        [TestMethod]
        public void ContentsAreChanged_WhereClause_FiltersProperly_ReturnsTrue()
        {
            var empJson = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var deptJson = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Department.json");

            var departmentsList = new List<Department>
            {
                JsonSerializer.Deserialize<Department>(deptJson),
                new Department { Id = Guid.NewGuid(), DisplayName = "Sales" },
                new Department { Id = Guid.NewGuid(), DisplayName = "CustomerSuccess" },
            };

            var employeesList = new List<Employee>
            {
                JsonSerializer.Deserialize<Employee>(empJson),
                JsonSerializer.Deserialize<Employee>(empJson),
            }.ToTrackedList();

            var testId = employeesList[0].Id = Guid.NewGuid();
            employeesList[0].Title = "Chief Idiot";

            employeesList.ContentsAreChanged(c => c.Id == testId).Should().BeTrue();
        }

        /// <summary>
        /// Testing that we make a change, but not to an item with the foreign keys we're looking for.
        /// </summary>
        [TestMethod]
        public void ContentsAreChanged_ForeignList_FiltersProperly_ReturnsFalse()
        {
            var empJson = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var deptJson = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Department.json");

            var departmentsList = new List<Department>
            {
                JsonSerializer.Deserialize<Department>(deptJson),
                new Department { Id = Guid.NewGuid(), DisplayName = "Sales" },
                new Department { Id = Guid.NewGuid(), DisplayName = "CustomerSuccess" },
            };

            var employeesList = new List<Employee>
            {
                JsonSerializer.Deserialize<Employee>(empJson),
                JsonSerializer.Deserialize<Employee>(empJson),
            };

            // RWM: Adjust one of the records to link to a different department.
            employeesList[0].DepartmentId = departmentsList[1].Id;
            //employeesList = employeesList.ToTrackedList();

            // RWM: Run a query that will never return a positive result.
            employeesList[0].Id = Guid.NewGuid();
            employeesList[0].Title = "Chief Idiot";

            employeesList.ContentsAreChanged(departmentsList, c => c.DepartmentId).Should().BeFalse();
        }

        [TestMethod]
        public void ContentsAreChanged_ForeignList_FiltersProperly_ReturnsTrue()
        {
            var empJson = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var deptJson = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Department.json");

            var departmentsList = new List<Department>
            {
                JsonSerializer.Deserialize<Department>(deptJson),
            };

            var employeesList = new List<Employee>
            {
                JsonSerializer.Deserialize<Employee>(empJson),
                JsonSerializer.Deserialize<Employee>(empJson),
            }.ToTrackedList();

            var testId = employeesList[1].DepartmentId = Guid.NewGuid();
            employeesList[0].Title = "Chief Idiot";

            employeesList.ContentsAreChanged(departmentsList, c => c.DepartmentId).Should().BeTrue();
        }

        #endregion

        #region ContainsId()

        [TestMethod]
        public void ContainsId_ReturnsTrue()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var list = new List<Employee>
            {
                JsonSerializer.Deserialize<Employee>(json),
                JsonSerializer.Deserialize<Employee>(json),
                JsonSerializer.Deserialize<Employee>(json),
            };

            list[0].Id = Guid.NewGuid();
            var idToTest = list[1].Id;
            list[2].Id = Guid.NewGuid();

            list.ContainsId(idToTest).Should().BeTrue();
        }

        [TestMethod]
        public void ContainsId_ReturnsFalse()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var list = new List<Employee>
            {
                JsonSerializer.Deserialize<Employee>(json),
                JsonSerializer.Deserialize<Employee>(json),
                JsonSerializer.Deserialize<Employee>(json),
            };

            list[0].Id = Guid.NewGuid();
            var idToTest = list[1].Id;
            list[1].Id = Guid.NewGuid();
            list[2].Id = Guid.NewGuid();

            list.ContainsId(idToTest).Should().BeFalse();
        }

        #endregion

        #region FilterForChanges()

        [TestMethod]
        public void FilterForChanges_FiltersProperly_ExpectsOne()
        {
            var empJson = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var deptJson = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Department.json");

            var departmentsList = new List<Department>
            {
                JsonSerializer.Deserialize<Department>(deptJson),
                new Department { Id = Guid.NewGuid(), DisplayName = "Sales" },
                new Department { Id = Guid.NewGuid(), DisplayName = "CustomerSuccess" },
            };

            var employeesList = new List<Employee>
            {
                JsonSerializer.Deserialize<Employee>(empJson),
                JsonSerializer.Deserialize<Employee>(empJson),
            }.ToTrackedList();

            employeesList[0].Id = Guid.NewGuid();
            employeesList[0].Title = "Chief Idiot";

            var changedItems = employeesList.FilterForChanges(departmentsList, c => c.DepartmentId);
            changedItems.Should().ContainSingle();
        }

        [TestMethod]
        public void FilterForChanges_FiltersProperly_ExpectsZero()
        {
            var empJson = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var deptJson = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Department.json");

            var salesId = Guid.NewGuid();
            var departmentsList = new List<Department>
            {
                JsonSerializer.Deserialize<Department>(deptJson),
                new Department { Id = Guid.NewGuid(), DisplayName = "Sales" },
                new Department { Id = Guid.NewGuid(), DisplayName = "CustomerSuccess" },
            };

            //RWM: Change the ID so nothing lines up.
            departmentsList[0].Id = Guid.NewGuid();

            var employeesList = new List<Employee>
            {
                JsonSerializer.Deserialize<Employee>(empJson),
                JsonSerializer.Deserialize<Employee>(empJson),
            }.ToTrackedList();

            employeesList[0].Id = Guid.NewGuid();
            employeesList[0].Title = "Chief Idiot";

            var changedItems = employeesList.FilterForChanges(departmentsList, c => c.DepartmentId);
            changedItems.Should().BeEmpty();
        }

        #endregion

        #region None()

        [TestMethod]
        public void None_EmptyList_Predicate_ReturnsTrue()
        {
            new List<KeyValuePair<string, string>>().None(c => c.Key == "Test").Should().BeTrue();
        }

        [TestMethod]
        public void None_List_NoPredicate_ReturnsFalse()
        {
            new List<string> { "Yo!" }.None().Should().BeFalse();
        }

        [TestMethod]
        public void None_List_Predicate_ReturnsFalse()
        {
            new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Test", "") }.None(c => c.Key == "Test").Should().BeFalse();
        }

        [TestMethod]
        public void None_NullObject_Predicate_ReturnsTrue()
        {
            (null as List<string>).None().Should().BeTrue();
        }

        [TestMethod]
        public void None_NullObject_NoPredicate_ReturnsTrue()
        {
            (null as List<string>).None().Should().BeTrue();
        }

        #endregion

        #region ToTrackedList()

        [TestMethod]
        public void ToTrackedList_ChangesTracked()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Person.json");
            var list = new List<Person>
            {
                JsonSerializer.Deserialize<Person>(json),
                JsonSerializer.Deserialize<Person>(json),
                JsonSerializer.Deserialize<Person>(json),
            };

            list.Should().OnlyContain(c => c.ShouldTrackChanges == false);
            list = list.ToTrackedList();
            list.Should().OnlyContain(c => c.ShouldTrackChanges == true);
        }

        [TestMethod]
        public void ToTrackedList_PropertyFunc_ChangesTracked()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Employee.json");
            var list = new List<Employee>
            {
                JsonSerializer.Deserialize<Employee>(json),
                JsonSerializer.Deserialize<Employee>(json),
                JsonSerializer.Deserialize<Employee>(json),
            };

            list.Should().OnlyContain(c => c.ShouldTrackChanges == false);
            list.Select(c => c.Person).Should().OnlyContain(c => c.ShouldTrackChanges == false);
            list = list.ToTrackedList(true);
            list.Should().OnlyContain(c => c.ShouldTrackChanges == true);
            list.Select(c => c.Person).Should().OnlyContain(c => c.ShouldTrackChanges == true);
        }

        [TestMethod]
        public void ToTrackedList_ListPropertyFunc_ChangesTracked()
        {
            var json = File.ReadAllText("..//..//..//..//CloudNimble.EasyAF.Tests.Core//Baselines//Concert.json");
            var list = new List<Concert>
            {
                JsonSerializer.Deserialize<Concert>(json),
                JsonSerializer.Deserialize<Concert>(json),
                JsonSerializer.Deserialize<Concert>(json),
            };

            list.Should().OnlyContain(c => c.ShouldTrackChanges == false);
            list.Select(c => c.Organizer).Should().OnlyContain(c => c.ShouldTrackChanges == false);
            list.SelectMany(c => c.Attendees).Should().OnlyContain(c => c.ShouldTrackChanges == false);

            list = list.ToTrackedList(true);

            list.Should().OnlyContain(c => c.ShouldTrackChanges == true);
            list.Select(c => c.Organizer).Should().OnlyContain(c => c.ShouldTrackChanges == true);
            list.SelectMany(c => c.Attendees).Should().OnlyContain(c => c.ShouldTrackChanges == true);
        }

        #endregion

    }

}
