using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CloudNimble.EasyAF.Tests.Http
{

    [TestClass]
    public class UriExtensionsTests
    {

        private const string testBase = "https://o.pizza";

        [TestMethod]
        public void ToODataUri_Filter_WithSpaces_DollarSign()
        {
            var uri = new Uri(testBase).ToODataUri(filter: "test eq true");
            uri.ToString().Should().Be($"{testBase}/?$filter=test+eq+true");
        }

        [TestMethod]
        public void ToODataUri_Filter_WithSpaces_NoDollarSign()
        {
            var uri = new Uri(testBase).ToODataUri(false, filter: "test eq true");
            uri.ToString().Should().Be($"{testBase}/?filter=test+eq+true");
        }

    }

}
