using CloudNimble.EasyAF.CodeGen;
using CloudNimble.EasyAF.CodeGen.Legacy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{

    [TestClass]
    public class CodeGenerationToolsTests : CodeGenTestBase
    {

        #region Private Members


        #endregion

        #region Properties

        public EdmxLoader EdmxLoader { get; private set; }

        #endregion

        #region Test Setup / Teardown

        [TestInitialize]
        public void Initialize()
        {
            EdmxLoader = new EdmxLoader(ModelPath);
            EdmxLoader.Load();
        }

        #endregion

        [TestMethod]
        public void NoEasyAFInterfaces()
        {
            var classString = CodeGenerationTools.EntityClassDeclaration(EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "User"));
            classString.Should().Be("public partial class User : DbObservableObject, IIdentifiable<Guid>, ICreatedAuditable, ICreatorTrackable<Guid>");
        }

        [TestMethod]
        public void IsStateInterface()
        {
            var classString = CodeGenerationTools.EntityClassDeclaration(EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "InquiryStateType"));
            classString.Should().Be("public partial class InquiryStateType : DbObservableObject, IDbStateEnum, ICreatedAuditable, ICreatorTrackable<Guid>, IUpdatedAuditable, IUpdaterTrackable<Guid>");
        }

        [TestMethod]
        public void IsStatusInterface()
        {
            var classString = CodeGenerationTools.EntityClassDeclaration(EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "ProductStatusType"));
            classString.Should().Be("public partial class ProductStatusType : DbObservableObject, IDbStatusEnum, ICreatedAuditable, ICreatorTrackable<Guid>, IUpdatedAuditable, IUpdaterTrackable<Guid>");
        }

        [TestMethod]
        public void HasStateInterface()
        {
            var classString = CodeGenerationTools.EntityClassDeclaration(EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "Inquiry"));
            classString.Should().Be("public partial class Inquiry : DbObservableObject, IHasState<InquiryStateType>, ICreatedAuditable, ICreatorTrackable<Guid>, IUpdatedAuditable, IUpdaterTrackable<Guid>");
        }

        [TestMethod]
        public void HasStatusInterface()
        {
            var classString = CodeGenerationTools.EntityClassDeclaration(EdmxLoader.Entities.FirstOrDefault(c => c.EntityType.Name == "Product"));
            classString.Should().Be("public partial class Product : DbObservableObject, IHasStatus<ProductStatusType>, ICreatedAuditable, ICreatorTrackable<Guid>, IUpdatedAuditable, IUpdaterTrackable<Guid>");
        }

    }

}
