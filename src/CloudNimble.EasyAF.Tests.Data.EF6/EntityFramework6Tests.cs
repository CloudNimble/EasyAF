using CloudNimble.EasyAF.Data;
using EasyAFModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;

namespace CloudNimble.EasyAF.Tests.Data.EF6
{

    [TestClass]
    public class EntityFramework6Tests
    {

        [TestMethod]
        public void EF6_ShouldConnectToDatabase()
        {
            DbConfiguration.SetConfiguration(new EasyAFSqlAzureConfiguration());
            var context = new EasyAFEntities("data source=(localdb)\\MSSQLLocalDb;initial catalog=EasyAF;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework");
            context.Database.Exists().Should().BeTrue();
            context.Inquiries.Should().NotBeNull();
        }

    }

}
