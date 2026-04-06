using CloudNimble.BurnRate.Tests.Business;
using EasyAFModel;
using EasyAFModel.Managers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace CloudNimble.EasyAF.Tests.Business
{
    [TestClass]
    [TestCategory("RequiresDatabase")]
    public class EntityManagerTests : EasyAFBusinessTestBase
    {

        #region Test Lifecycle

        /// <summary>
        /// Sets up services needed for tests.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            SetClaimsPrincipalSelectorToThreadPrincipal(new Claim(EasyAF_ClaimsPrincipalExtensions.NameClaimType, "731c7991-8714-4a6a-a98f-311f6e79f742"));
            TestSetup();
        }

        #endregion

        [TestMethod]
        public async Task ProductManager_IdIsPopulated()
        {
            var dbContext = GetScopedService<EasyAFEntities>();

            var entity = new Product
            {
                DisplayName = $"UnitTest_{DateTime.Now}"
            };

            var manager = new ProductManager(dbContext, null);
            Func<Task> test = async () =>
            {
                //try
                //{
                await manager.OnInsertingAsync(entity);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
                //}
            };
            await test.Should().NotThrowAsync<Exception>();
            entity.Id.Should().NotBeEmpty();
        }

        [TestMethod]
        public async Task ProductManager_CreatedByIdIsPopulated()
        {
            var dbContext = GetScopedService<EasyAFEntities>();

            var entity = new Product
            {
                DisplayName = $"UnitTest_{DateTime.Now}"
            };

            var manager = new ProductManager(dbContext, null);
            Func<Task> test = async () =>
            {
                //try
                //{
                await manager.OnInsertingAsync(entity);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
                //}
            };
            await test.Should().NotThrowAsync<Exception>();
            entity.CreatedById.Should().Be(new Guid("731c7991-8714-4a6a-a98f-311f6e79f742"));
        }

        /// <summary>
        /// Tests that the DeleteByStatusType() call correctly executes.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ProductManager_CanDeleteByExpression()
        {
            var keepStatusTypeId = Guid.NewGuid();
            var deleteStatusTypeId = Guid.NewGuid();

            var dbContext = GetScopedService<EasyAFEntities>();

            var statuses = dbContext.ProductStatusTypes.Where(c => c.SortOrder < 2).ToList();
            if (!statuses.Any(c => c.SortOrder == 0))
            {
                dbContext.ProductStatusTypes.Add(new ProductStatusType { Id = keepStatusTypeId, DisplayName = "Started", SortOrder = 0 });
                await dbContext.SaveChangesAsync();
            }
            else
            {
                keepStatusTypeId = statuses.First(c => c.SortOrder == 0).Id;
            }

            if (!statuses.Any(c => c.SortOrder == 1))
            {
                dbContext.ProductStatusTypes.Add(new ProductStatusType { Id = deleteStatusTypeId, DisplayName = "Reviewed", SortOrder = 1 });
                await dbContext.SaveChangesAsync();
            }
            else
            {
                deleteStatusTypeId = statuses.First(c => c.SortOrder == 1).Id;
            }

            dbContext.Products.AddRange(
            [
                new() { Id = Guid.NewGuid(), DisplayName= "DeleteUnitTest", StatusTypeId = keepStatusTypeId },
                new() { Id = Guid.NewGuid(), DisplayName= "DeleteUnitTest", StatusTypeId = deleteStatusTypeId },
                new() { Id = Guid.NewGuid(), DisplayName= "DeleteUnitTest", StatusTypeId = deleteStatusTypeId }
            ]);

            await dbContext.SaveChangesAsync();

            var manager = new ProductManager(dbContext, null);
            var count = await manager.DeleteByStatusType(deleteStatusTypeId);

            count.Should().Be(2);
            dbContext.Products.Should().NotContain(c => c.StatusTypeId == deleteStatusTypeId);
            dbContext.Products.Should().HaveCount(1);

            await dbContext.Products.DeleteAsync();
        }


        /// <summary>
        /// Tests that the DeleteByStatusType() call correctly executes.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ProductManager_ResetAuditProperties_CanResetProduct()
        {
            var dbContext = GetScopedService<EasyAFEntities>();

            var product = new Product
            {
                DisplayName = "Test",
            };

            var manager = new ProductManager(dbContext, null);
            await manager.InsertAsync(product, false);

            product.CreatedById.Should().NotBeEmpty();
            product.DateCreated.Should().BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));

            await manager.UpdateAsync(product, false);
            product.UpdatedById.Should().NotBeEmpty();
            product.DateUpdated.Should().BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));

            manager.ResetAuditProperties(product);

            product.CreatedById.Should().NotBeEmpty();
            product.DateCreated.Should().BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
            product.UpdatedById.Should().BeNull();
            product.DateUpdated.Should().BeNull();
        }

        /// <summary>
        /// Tests that the DeleteByStatusType() call correctly executes.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ProductManager_ResetAuditProperties_CanResetInquiry()
        {
            var dbContext = GetScopedService<EasyAFEntities>();

            var inquiry = new Inquiry
            {
                Message = "Test",
            };

            var manager = new ProductManager(dbContext, null);
            var inquiryManager = new InquiryManager(dbContext, null);

            await inquiryManager.InsertAsync(inquiry, false);

            inquiry.CreatedById.Should().NotBeEmpty();
            inquiry.DateCreated.Should().BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));

            await inquiryManager.UpdateAsync(inquiry, false);
            inquiry.UpdatedById.Should().NotBeEmpty();
            inquiry.DateUpdated.Should().BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));

            manager.ResetAuditProperties(inquiry);

            inquiry.CreatedById.Should().NotBeEmpty();
            inquiry.DateCreated.Should().BeCloseTo(DateTimeOffset.Now, new TimeSpan(0, 0, 1));
            inquiry.UpdatedById.Should().BeNull();
            inquiry.DateUpdated.Should().BeNull();
        }

        //RWM: Reset object tests here:
        //1) test for this manager
        //    2) test for a different object  in this manager.

    }
}
