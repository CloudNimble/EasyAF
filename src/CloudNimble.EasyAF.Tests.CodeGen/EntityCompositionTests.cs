using CloudNimble.EasyAF.CodeGen;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{
    [TestClass]
    public class EntityCompositionTests : CodeGenTestBase
    {

        [TestMethod]
        public void HasStateType()
        {
            var loader = new EdmxLoader(ModelPath);
            loader.FilePath.Should().NotBeNullOrWhiteSpace();

            loader.Load();
            loader.EdmxSchemaErrors.Should().BeEmpty();
            loader.ModelNamespace.Should().NotBeNullOrWhiteSpace();
            loader.EdmItems.Should().NotBeEmpty();

            var entity = loader.Entities.FirstOrDefault(c => c.EntityType.Name == "Inquiry");
            entity.Should().NotBeNull();

            entity.EntityType.Should().NotBeNull();
            entity.CollectionNavigationProperties.Should().HaveCount(0);
            entity.ComplexProperties.Should().HaveCount(0);
            entity.HasState.Should().BeTrue();
            entity.HasStatus.Should().BeFalse();
            entity.IsActiveTrackable.Should().BeFalse();
            entity.IsCreatedAuditable.Should().BeTrue();
            entity.IsCreatorTrackable.Should().BeTrue();
            entity.IsDbEnum.Should().BeFalse();
            entity.IsDbStateEnum.Should().BeFalse();
            entity.IsDbStatusEnum.Should().BeFalse();
            entity.IsHumanReadable.Should().BeFalse();
            entity.IsIdentifiable.Should().BeTrue();
            entity.IsSortable.Should().BeFalse();
            entity.IsUpdatedAuditable.Should().BeTrue();
            entity.IsUpdaterTrackable.Should().BeTrue();
            entity.KeyProperties.Should().HaveCount(1);
            entity.NavigationProperties.Should().HaveCount(2);
            entity.OtherProperties.Should().HaveCount(2);
            entity.PropertiesWithDefaults.Should().HaveCount(0);
            entity.SimpleProperties.Should().HaveCount(8);
        }

        [TestMethod]
        public void HasStatusType()
        {
            var loader = new EdmxLoader(ModelPath);
            loader.FilePath.Should().NotBeNullOrWhiteSpace();

            loader.Load();
            loader.EdmxSchemaErrors.Should().BeEmpty();
            loader.ModelNamespace.Should().NotBeNullOrWhiteSpace();
            loader.EdmItems.Should().NotBeEmpty();

            var entity = loader.Entities.FirstOrDefault(c => c.EntityType.Name == "Product");
            entity.Should().NotBeNull();

            entity.EntityType.Should().NotBeNull();
            entity.CollectionNavigationProperties.Should().HaveCount(0);
            entity.ComplexProperties.Should().HaveCount(0);
            entity.HasState.Should().BeFalse();
            entity.HasStatus.Should().BeTrue();
            entity.IsActiveTrackable.Should().BeFalse();
            entity.IsCreatedAuditable.Should().BeTrue();
            entity.IsCreatorTrackable.Should().BeTrue();
            entity.IsDbEnum.Should().BeFalse();
            entity.IsDbStateEnum.Should().BeFalse();
            entity.IsDbStatusEnum.Should().BeFalse();
            entity.IsHumanReadable.Should().BeTrue();
            entity.IsIdentifiable.Should().BeTrue();
            entity.IsSortable.Should().BeFalse();
            entity.IsUpdatedAuditable.Should().BeTrue();
            entity.IsUpdaterTrackable.Should().BeTrue();
            entity.KeyProperties.Should().HaveCount(1);
            entity.NavigationProperties.Should().HaveCount(1);
            entity.OtherProperties.Should().HaveCount(0);
            entity.PropertiesWithDefaults.Should().HaveCount(0);
            entity.SimpleProperties.Should().HaveCount(7);
        }

        [TestMethod]
        public void IsStateType()
        {
            var loader = new EdmxLoader(ModelPath);
            loader.FilePath.Should().NotBeNullOrWhiteSpace();

            loader.Load();
            loader.EdmxSchemaErrors.Should().BeEmpty();
            loader.ModelNamespace.Should().NotBeNullOrWhiteSpace();
            loader.EdmItems.Should().NotBeEmpty();

            var entity = loader.Entities.FirstOrDefault(c => c.EntityType.Name == "InquiryStateType");
            entity.Should().NotBeNull();

            entity.EntityType.Should().NotBeNull();
            entity.CollectionNavigationProperties.Should().HaveCount(1);
            entity.ComplexProperties.Should().HaveCount(0);
            entity.HasState.Should().BeFalse();
            entity.HasStatus.Should().BeFalse();
            entity.IsActiveTrackable.Should().BeTrue();
            entity.IsCreatedAuditable.Should().BeTrue();
            entity.IsCreatorTrackable.Should().BeTrue();
            entity.IsDbEnum.Should().BeTrue();
            entity.IsDbStateEnum.Should().BeTrue();
            entity.IsDbStatusEnum.Should().BeFalse();
            entity.IsHumanReadable.Should().BeTrue();
            entity.IsIdentifiable.Should().BeTrue();
            entity.IsSortable.Should().BeTrue();
            entity.IsUpdatedAuditable.Should().BeTrue();
            entity.IsUpdaterTrackable.Should().BeTrue();
            entity.KeyProperties.Should().HaveCount(1);
            entity.NavigationProperties.Should().HaveCount(1);
            entity.OtherProperties.Should().HaveCount(0);
            entity.PropertiesWithDefaults.Should().HaveCount(0);
            entity.SimpleProperties.Should().HaveCount(13);
        }

        [TestMethod]
        public void IsStatusType()
        {
            var loader = new EdmxLoader(ModelPath);
            loader.FilePath.Should().NotBeNullOrWhiteSpace();

            loader.Load();
            loader.EdmxSchemaErrors.Should().BeEmpty();
            loader.ModelNamespace.Should().NotBeNullOrWhiteSpace();
            loader.EdmItems.Should().NotBeEmpty();

            var entity = loader.Entities.FirstOrDefault(c => c.EntityType.Name == "ProductStatusType");
            entity.Should().NotBeNull();

            entity.EntityType.Should().NotBeNull();
            entity.CollectionNavigationProperties.Should().HaveCount(1);
            entity.ComplexProperties.Should().HaveCount(0);
            entity.HasState.Should().BeFalse();
            entity.HasStatus.Should().BeFalse();
            entity.IsActiveTrackable.Should().BeTrue();
            entity.IsCreatedAuditable.Should().BeTrue();
            entity.IsCreatorTrackable.Should().BeTrue();
            entity.IsDbEnum.Should().BeTrue();
            entity.IsDbStateEnum.Should().BeFalse();
            entity.IsDbStatusEnum.Should().BeTrue();
            entity.IsHumanReadable.Should().BeTrue();
            entity.IsIdentifiable.Should().BeTrue();
            entity.IsSortable.Should().BeTrue();
            entity.IsUpdatedAuditable.Should().BeTrue();
            entity.IsUpdaterTrackable.Should().BeTrue();
            entity.KeyProperties.Should().HaveCount(1);
            entity.NavigationProperties.Should().HaveCount(1);
            entity.OtherProperties.Should().HaveCount(0);
            entity.PropertiesWithDefaults.Should().HaveCount(0);
            entity.SimpleProperties.Should().HaveCount(8);
        }

        [TestMethod]
        public void Simple()
        {
            var loader = new EdmxLoader(ModelPath);
            loader.FilePath.Should().NotBeNullOrWhiteSpace();

            loader.Load();
            loader.EdmxSchemaErrors.Should().BeEmpty();
            loader.ModelNamespace.Should().NotBeNullOrWhiteSpace();
            loader.EdmItems.Should().NotBeEmpty();

            var entity = loader.Entities.FirstOrDefault(c => c.EntityType.Name == "User");
            entity.Should().NotBeNull();

            entity.EntityType.Should().NotBeNull();
            entity.CollectionNavigationProperties.Should().HaveCount(1);
            entity.ComplexProperties.Should().HaveCount(0);
            entity.HasState.Should().BeFalse();
            entity.HasStatus.Should().BeFalse();
            entity.IsActiveTrackable.Should().BeFalse();
            entity.IsCreatedAuditable.Should().BeTrue();
            entity.IsCreatorTrackable.Should().BeTrue();
            entity.IsDbEnum.Should().BeFalse();
            entity.IsDbStateEnum.Should().BeFalse();
            entity.IsDbStatusEnum.Should().BeFalse();
            entity.IsHumanReadable.Should().BeFalse();
            entity.IsIdentifiable.Should().BeTrue();
            entity.IsSortable.Should().BeFalse();
            entity.IsUpdatedAuditable.Should().BeFalse();
            entity.IsUpdaterTrackable.Should().BeFalse();
            entity.KeyProperties.Should().HaveCount(1);
            entity.NavigationProperties.Should().HaveCount(1);
            entity.OtherProperties.Should().HaveCount(2);
            entity.PropertiesWithDefaults.Should().HaveCount(0);
            entity.SimpleProperties.Should().HaveCount(5);
        }

    }

}
