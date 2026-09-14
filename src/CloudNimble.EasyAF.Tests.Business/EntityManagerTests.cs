using CloudNimble.BurnRate.Tests.Business;
using EasyAFModel;
using EasyAFModel.Managers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace CloudNimble.EasyAF.Tests.Business
{
    [TestClass]
    [TestCategory("RequiresDatabase")]
    [DoNotParallelize]
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

        [TestCleanup]
        public void TestCleanup()
        {
            TestTearDown();
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
            var dbContext = GetScopedService<EasyAFEntities>();
            var keepStatusTypeId = Guid.NewGuid();
            var deleteStatusTypeId = Guid.NewGuid();
            var keepProductId = Guid.NewGuid();
            var deleteProductId1 = Guid.NewGuid();
            var deleteProductId2 = Guid.NewGuid();
            var productIds = new[] { keepProductId, deleteProductId1, deleteProductId2 };

            try
            {
                dbContext.ProductStatusTypes.AddRange(
                [
                    new ProductStatusType
                    {
                        Id = keepStatusTypeId,
                        DisplayName = $"Keep_{keepStatusTypeId:N}",
                        SortOrder = int.MaxValue - 1,
                        IsActive = true,
                        DateCreated = DateTimeOffset.UtcNow
                    },
                    new ProductStatusType
                    {
                        Id = deleteStatusTypeId,
                        DisplayName = $"Delete_{deleteStatusTypeId:N}",
                        SortOrder = int.MaxValue,
                        IsActive = true,
                        DateCreated = DateTimeOffset.UtcNow
                    }
                ]);
                await dbContext.SaveChangesAsync();

                dbContext.Products.AddRange(
                [
                    new() { Id = keepProductId, DisplayName = $"Keep_{keepProductId:N}", StatusTypeId = keepStatusTypeId },
                    new() { Id = deleteProductId1, DisplayName = $"Delete_{deleteProductId1:N}", StatusTypeId = deleteStatusTypeId },
                    new() { Id = deleteProductId2, DisplayName = $"Delete_{deleteProductId2:N}", StatusTypeId = deleteStatusTypeId }
                ]);
                await dbContext.SaveChangesAsync();

                var manager = new ProductManager(dbContext, null);
                var count = await manager.DeleteByStatusType(deleteStatusTypeId);

                count.Should().Be(2);

                var remainingIds = dbContext.Products.AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToList();

                remainingIds.Should().ContainSingle().Which.Should().Be(keepProductId);
            }
            finally
            {
                await dbContext.Products.Where(p => productIds.Contains(p.Id)).DeleteAsync();
                await dbContext.ProductStatusTypes
                    .Where(s => s.Id == keepStatusTypeId || s.Id == deleteStatusTypeId)
                    .DeleteAsync();
            }
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

            try
            {
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
            finally
            {
                if (product.Id != Guid.Empty)
                {
                    await dbContext.Products.Where(p => p.Id == product.Id).DeleteAsync();
                }
            }
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

            try
            {
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
            finally
            {
                if (inquiry.Id != Guid.Empty)
                {
                    await dbContext.Inquiries.Where(i => i.Id == inquiry.Id).DeleteAsync();
                }
            }
        }

        //RWM: Reset object tests here:
        //1) test for this manager
        //    2) test for a different object  in this manager.

    }
}
