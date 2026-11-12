using CloudNimble.EasyAF.Business;
using CloudNimble.EasyAF.Core;
using EasyAFModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CloudNimble.EasyAF.Tests.Business
{

    /// <summary>
    /// Tests that <see cref="EntityManager{TContext, TEntity}"/> fills creator and updater IDs for any value-type ID, not just
    /// <see cref="Guid"/>. These tests never touch the database, so the managers are created without a DbContext.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class EntityManagerIdTypeTests
    {

        #region Fields

        private static readonly Guid UserGuid = new("731c7991-8714-4a6a-a98f-311f6e79f742");

        private Func<ClaimsPrincipal> _originalSelector;

        #endregion

        #region Test Setup

        /// <summary>
        /// Remembers the global ClaimsPrincipal selector so each test can replace it.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _originalSelector = ClaimsPrincipal.ClaimsPrincipalSelector;
        }

        /// <summary>
        /// Restores the global ClaimsPrincipal selector.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            ClaimsPrincipal.ClaimsPrincipalSelector = _originalSelector;
        }

        /// <summary>
        /// Makes <see cref="ClaimsPrincipal.Current"/> return a user whose NameIdentifier claim is <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The claim value, or <see langword="null"/> for a user with no ID claim.</param>
        private static void SignIn(string userId)
        {
            var claims = userId is null ? new List<Claim>() : new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
            ClaimsPrincipal.ClaimsPrincipalSelector = () => principal;
        }

        #endregion

        #region OnInsertingAsync Tests

        [TestMethod]
        public async Task OnInsertingAsync_WithIntCreator_ShouldSetCreatedById()
        {
            SignIn("42");
            var entity = new IntTrackedEntity();

            await new IntTrackedEntityManager().OnInsertingAsync(entity);

            entity.CreatedById.Should().Be(42);
            entity.DateCreated.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        }

        [TestMethod]
        public async Task OnInsertingAsync_WithGuidCreator_ShouldSetCreatedById()
        {
            SignIn(UserGuid.ToString());
            var entity = new GuidTrackedEntity();

            await new GuidTrackedEntityManager().OnInsertingAsync(entity);

            entity.CreatedById.Should().Be(UserGuid);
        }

        [TestMethod]
        public async Task OnInsertingAsync_WithNoIdClaim_ShouldLeaveCreatedByIdUnset()
        {
            SignIn(null);
            var entity = new GuidTrackedEntity();

            await new GuidTrackedEntityManager().OnInsertingAsync(entity);

            entity.CreatedById.Should().Be(Guid.Empty);
        }

        [TestMethod]
        public async Task OnInsertingAsync_WithClaimOfTheWrongType_ShouldLeaveCreatedByIdUnset()
        {
            SignIn(UserGuid.ToString());
            var entity = new IntTrackedEntity();

            Func<Task> act = () => new IntTrackedEntityManager().OnInsertingAsync(entity);

            await act.Should().NotThrowAsync();
            entity.CreatedById.Should().Be(0);
        }

        #endregion

        #region OnUpdatingAsync Tests

        [TestMethod]
        public async Task OnUpdatingAsync_WithIntUpdater_ShouldSetUpdatedById()
        {
            SignIn("42");
            var entity = new IntTrackedEntity();

            await new IntTrackedEntityManager().OnUpdatingAsync(entity);

            entity.UpdatedById.Should().Be(42);
            entity.DateUpdated.Should().NotBeNull();
        }

        [TestMethod]
        public async Task OnUpdatingAsync_WithGuidUpdater_ShouldSetUpdatedById()
        {
            SignIn(UserGuid.ToString());
            var entity = new GuidTrackedEntity();

            await new GuidTrackedEntityManager().OnUpdatingAsync(entity);

            entity.UpdatedById.Should().Be(UserGuid);
        }

        #endregion

        #region ResetAuditProperties Tests

        [TestMethod]
        public void ResetAuditProperties_WithIntIds_ShouldSetCreatorAndClearUpdater()
        {
            SignIn("42");
            var entity = new IntTrackedEntity { UpdatedById = 7, DateUpdated = DateTimeOffset.UtcNow };

            new IntTrackedEntityManager().ResetAuditProperties(entity);

            entity.CreatedById.Should().Be(42);
            entity.UpdatedById.Should().BeNull();
            entity.DateUpdated.Should().BeNull();
        }

        [TestMethod]
        public void ResetAuditProperties_WithNoCurrentPrincipal_ShouldNotThrow()
        {
            ClaimsPrincipal.ClaimsPrincipalSelector = () => null;
            var entity = new IntTrackedEntity { UpdatedById = 7 };

            Action act = () => new IntTrackedEntityManager().ResetAuditProperties(entity);

            act.Should().NotThrow();
            entity.UpdatedById.Should().BeNull();
        }

        #endregion

        #region Test Types

        /// <summary>
        /// An entity with <see cref="int"/> creator and updater IDs.
        /// </summary>
        public class IntTrackedEntity : DbObservableObject, ICreatorTrackable<int>, IUpdaterTrackable<int>, ICreatedAuditable, IUpdatedAuditable
        {
            public int CreatedById { get; set; }

            public int? UpdatedById { get; set; }

            public DateTimeOffset DateCreated { get; set; }

            public DateTimeOffset? DateUpdated { get; set; }
        }

        /// <summary>
        /// An entity with <see cref="Guid"/> creator and updater IDs.
        /// </summary>
        public class GuidTrackedEntity : DbObservableObject, ICreatorTrackable<Guid>, IUpdaterTrackable<Guid>, ICreatedAuditable, IUpdatedAuditable
        {
            public Guid CreatedById { get; set; }

            public Guid? UpdatedById { get; set; }

            public DateTimeOffset DateCreated { get; set; }

            public DateTimeOffset? DateUpdated { get; set; }
        }

        /// <summary>
        /// A manager for <see cref="IntTrackedEntity"/> that never touches the database.
        /// </summary>
        public class IntTrackedEntityManager : EntityManager<EasyAFEntities, IntTrackedEntity>
        {
            public IntTrackedEntityManager() : base(null, null) { }
        }

        /// <summary>
        /// A manager for <see cref="GuidTrackedEntity"/> that never touches the database.
        /// </summary>
        public class GuidTrackedEntityManager : EntityManager<EasyAFEntities, GuidTrackedEntity>
        {
            public GuidTrackedEntityManager() : base(null, null) { }
        }

        #endregion

    }

}
