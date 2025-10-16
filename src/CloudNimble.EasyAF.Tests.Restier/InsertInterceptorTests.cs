using CloudNimble.Breakdance.AspNetCore;
using EasyAFModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.Restier
{

    ///// <summary>
    ///// 
    ///// </summary>
    //[TestClass]
    //public class InsertInterceptorTests : EasyAFContextApiTestBase
    //{

    //    /// <summary>
    //    /// Initializes the test environment with user credentials.
    //    /// </summary>
    //    [TestInitialize]
    //    public void TestInitialize()
    //    {
    //        TestSetup();
    //        // RWM: Run this before the tests in case a previous test run failed to clean things up.
    //        TestCleanup();
    //        var db = GetApiInstance().DbContext;
            
    //        if (!db.ProductStatusTypes.Any())
    //        {
    //            db.ProductStatusTypes.Add(new ProductStatusType { Id = Guid.NewGuid(), DisplayName = "Started", SortOrder = 0 });
    //            db.SaveChanges();
    //        }
    //    }

    //    [TestCleanup]
    //    public void TestCleanup()
    //    {
    //        var db = GetApiInstance().DbContext;
            
    //        if (db.Products.Any())
    //        {
    //            db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Products]");
    //        }

    //    }

    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    [TestMethod]
    //    public async Task Products_Insert_IdIsNotNull()
    //    {
    //        var api = GetApiInstance();
    //        var db = api.DbContext;
    //        var statusType = db.ProductStatusTypes.First();

    //        var response = await ExecuteTestRequest(HttpMethod.Post, resource: "Products", acceptHeader: WebApiConstants.DefaultAcceptHeader, 
    //            payload: new Product { DisplayName = "test", StatusTypeId = statusType.Id }, jsonSerializerOptions: new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault });
    //        response.Should().NotBeNull();

    //        var content = await TestContext.LogAndReturnMessageContentAsync(response);
    //        content.Should().NotBeNullOrWhiteSpace().And.NotContain("error");

    //        var result = JsonSerializer.Deserialize<Product>(content);
    //        result.Id.Should().NotBeEmpty();
    //        result.DisplayName.Should().Be("test");
    //        result.StatusTypeId.Should().NotBeEmpty();
    //    }

    //}

}
