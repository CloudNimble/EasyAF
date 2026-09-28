using CloudNimble.EasyAF.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.Analyzers
{

    /// <summary>
    /// Proves the EasyAF source generator loads in a consuming project and compiles entities from
    /// <c>Tests.Shared/EntityModel.edmx</c> into this assembly. The project sets <c>EasyAFProjectType=Core</c> and
    /// <c>RootNamespace=Testing.OneTwoThree.Four</c>, so the entities land in <c>Testing.OneTwoThree.Four.Core</c>.
    /// </summary>
    [TestClass]
    public class GeneratedEntityTests
    {

        #region Fields

        private const string GeneratedNamespace = "Testing.OneTwoThree.Four.Core";

        #endregion

        #region Test Helpers

        /// <summary>
        /// Finds a generated type in this test assembly by its simple name.
        /// </summary>
        /// <param name="name">The entity name from the EDMX.</param>
        /// <returns>The type, or <see langword="null"/> if the generator did not produce it.</returns>
        private static Type GeneratedType(string name)
        {
            return typeof(GeneratedEntityTests).Assembly.GetType($"{GeneratedNamespace}.{name}");
        }

        #endregion

        #region Generated Entity Tests

        [TestMethod]
        public void Generator_ShouldCompileEveryEdmxEntityIntoThisAssembly()
        {
            var generated = typeof(GeneratedEntityTests).Assembly.GetTypes()
                .Where(t => t.Namespace == GeneratedNamespace)
                .Select(t => t.Name);

            generated.Should().BeEquivalentTo("Inquiry", "InquiryStateType", "Product", "ProductStatusType", "User");
        }

        [TestMethod]
        [DataRow("Inquiry")]
        [DataRow("InquiryStateType")]
        [DataRow("Product")]
        [DataRow("ProductStatusType")]
        [DataRow("User")]
        public void GeneratedEntity_ShouldBeAnObservableEasyAFEntity(string name)
        {
            var type = GeneratedType(name);

            type.Should().NotBeNull(because: $"the generator should emit '{name}'");
            type.Should().BeAssignableTo<DbObservableObject>();
        }

        [TestMethod]
        public void GeneratedInquiry_ShouldTrackItsStateAndAudit()
        {
            var inquiry = GeneratedType("Inquiry");
            var stateType = GeneratedType("InquiryStateType");

            inquiry.Should().Implement(typeof(IHasState<>).MakeGenericType(stateType));
            inquiry.Should().Implement<ICreatedAuditable>();
            inquiry.Should().Implement<IUpdatedAuditable>();
        }

        #endregion

    }

}
