using CloudNimble.EasyAF.Core;
using CloudNimble.EasyAF.Tests.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CloudNimble.EasyAF.Tests.Core
{

    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class NameOfTests
    {

        [TestMethod]
        public void NameOf_OneDeep()
        {
            NameOf.Full<Interval<int>>(c => c.Value).Should().Be("Value");
        }

        [TestMethod]
        public void NameOf_TwoDeep()
        {
            NameOf.Full<NameOfModels>(c => c.ChildEntity.HelloWorld).Should().Be("ChildEntity.HelloWorld");
        }

        [TestMethod]
        public void NameOf_TwoDeep_SlashSeparator()
        {
            NameOf.Full<NameOfModels>(c => c.ChildEntity.HelloWorld, "/").Should().Be("ChildEntity/HelloWorld");
        }

        [TestMethod]
        public void NameOf_TwoDeep_Prefix()
        {
            NameOf.Full<NameOfModels>("test", c => c.ChildEntity.HelloWorld).Should().Be("test.ChildEntity.HelloWorld");
        }

        [TestMethod]
        public void NameOf_TwoDeep_Prefix_SlashSeparator()
        {
            NameOf.Full<NameOfModels>("test", c => c.ChildEntity.HelloWorld, "/").Should().Be("test/ChildEntity/HelloWorld");
        }

    }

}
