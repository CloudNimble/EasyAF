using CloudNimble.EasyAF.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.Core
{

    [TestClass]
    public class IIdentifiableEqualityComparerTests
    {

        [TestMethod]
        public void IIdentifiableEqualityComparer_ObjectsAreEqual()
        {
            var x = new TestEntity(1);
            var y = new TestEntity(1);

            x.GetHashCode().Should().NotBe(y.GetHashCode());

            var comparer = new IIdentifiableEqualityComparer<int>();
            comparer.Equals(x, y).Should().BeTrue();
        }

        [TestMethod]
        public void IIdentifiableEqualityComparer_ObjectsAreNotEqual()
        {
            var x = new TestEntity(1);
            var y = new TestEntity(2);

            x.GetHashCode().Should().NotBe(y.GetHashCode());

            var comparer = new IIdentifiableEqualityComparer<int>();
            comparer.Equals(x, y).Should().BeFalse();
        }


        [TestMethod]
        public void IIdentifiableEqualityComparer_GroupsCorrectly()
        {
            var source = new List<TestEntity>
            {
                new TestEntity(1),
                new TestEntity(2),
                new TestEntity(1)
            };

            var groups = source.GroupBy(c => c, new IIdentifiableEqualityComparer<int>());
            groups.Should().HaveCount(2);
            groups.First().Should().HaveCount(2);
            groups.First().Should().OnlyContain(c => c.Id == 1);
            groups.Last().Should().HaveCount(1);
            groups.Last().Should().OnlyContain(c => c.Id == 2);
        }

    }

    public class TestEntity : IIdentifiable<int>
    {
        public int Id { get; set; }

        public TestEntity(int id)
        {
            Id = id;
        }
    }

}
