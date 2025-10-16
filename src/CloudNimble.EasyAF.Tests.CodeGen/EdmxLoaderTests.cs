using CloudNimble.EasyAF.CodeGen;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CloudNimble.EasyAF.Tests.CodeGen.Core
{
    [TestClass]
    public class EdmxLoaderTests : CodeGenTestBase
    {

        [TestMethod]
        public void CanLoadSampleEdmx()
        {
            var loader = new EdmxLoader(ModelPath);
            loader.FilePath.Should().NotBeNullOrWhiteSpace();

            loader.Load();
            loader.EdmxSchemaErrors.Should().BeEmpty();
            loader.ModelNamespace.Should().NotBeNullOrWhiteSpace();
            loader.EdmItems.Should().NotBeEmpty();
            loader.Entities.Should().HaveCount(5);
            loader.EntitySets.Should().NotBeEmpty();
        }

        [TestMethod]
        public void CanLoadEdmxWithDateOnlyProperty()
        {
            // Arrange
            var edmx = """
                <?xml version="1.0" encoding="utf-8"?>
                <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
                    <edmx:Runtime>
                        <edmx:StorageModels>
                            <Schema Namespace="TestModel.Store" Provider="System.Data.SqlClient" ProviderManifestToken="2012" xmlns="http://schemas.microsoft.com/ado/2009/11/edm/ssdl">
                                <EntityType Name="Events">
                                    <Key>
                                        <PropertyRef Name="Id" />
                                    </Key>
                                    <Property Name="Id" Type="int" Nullable="false" />
                                    <Property Name="EventDate" Type="date" Nullable="false" />
                                </EntityType>
                                <EntityContainer Name="TestModelStoreContainer">
                                    <EntitySet Name="Events" EntityType="TestModel.Store.Events" />
                                </EntityContainer>
                            </Schema>
                        </edmx:StorageModels>
                        <edmx:ConceptualModels>
                            <Schema Namespace="TestModel" Alias="Self" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
                                <EntityType Name="Event">
                                    <Key>
                                        <PropertyRef Name="Id" />
                                    </Key>
                                    <Property Name="Id" Type="Int32" Nullable="false" />
                                    <Property Name="EventDate" Type="DateOnly" Nullable="false" />
                                </EntityType>
                                <EntityContainer Name="TestContext">
                                    <EntitySet Name="Events" EntityType="Self.Event" />
                                </EntityContainer>
                            </Schema>
                        </edmx:ConceptualModels>
                        <edmx:Mappings>
                            <Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2009/11/mapping/cs">
                                <EntityContainerMapping StorageEntityContainer="TestModelStoreContainer" CdmEntityContainer="TestContext">
                                    <EntitySetMapping Name="Events">
                                        <EntityTypeMapping TypeName="TestModel.Event">
                                            <MappingFragment StoreEntitySet="Events">
                                                <ScalarProperty Name="Id" ColumnName="Id" />
                                                <ScalarProperty Name="EventDate" ColumnName="EventDate" />
                                            </MappingFragment>
                                        </EntityTypeMapping>
                                    </EntitySetMapping>
                                </EntityContainerMapping>
                            </Mapping>
                        </edmx:Mappings>
                    </edmx:Runtime>
                </edmx:Edmx>
                """;

            // Act
            var loader = new EdmxLoader();
            loader.Load(edmx);

            // Assert
            loader.EdmxSchemaErrors.Should().BeEmpty();
            loader.Entities.Should().HaveCount(1);
            
            var eventEntity = loader.Entities.First();
            eventEntity.EntityType.Name.Should().Be("Event");
            
            var eventDateProperty = eventEntity.EntityType.Properties.FirstOrDefault(p => p.Name == "EventDate");
            eventDateProperty.Should().NotBeNull();
            // The property should be recognized as DateOnly type
        }

        [TestMethod]
        public void CanLoadEdmxWithTimeOnlyProperty()
        {
            // Arrange
            var edmx = """
  <?xml version="1.0" encoding="utf-8"?>
  <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
      <edmx:Runtime>
          <edmx:StorageModels>
              <Schema Namespace="TestModel.Store" Provider="System.Data.SqlClient" ProviderManifestToken="2012"
  xmlns="http://schemas.microsoft.com/ado/2009/11/edm/ssdl">
                  <EntityType Name="Schedules">
                      <Key>
                          <PropertyRef Name="Id" />
                      </Key>
                      <Property Name="Id" Type="int" Nullable="false" />
                      <Property Name="StartTime" Type="time" Nullable="false" Precision="7" />
                  </EntityType>
                  <EntityContainer Name="TestModelStoreContainer">
                      <EntitySet Name="Schedules" EntityType="TestModel.Store.Schedules" />
                  </EntityContainer>
              </Schema>
          </edmx:StorageModels>
          <edmx:ConceptualModels>
              <Schema Namespace="TestModel" Alias="Self" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
                  <EntityType Name="Schedule">
                      <Key>
                          <PropertyRef Name="Id" />
                      </Key>
                      <Property Name="Id" Type="Int32" Nullable="false" />
                      <Property Name="StartTime" Type="TimeOnly" Nullable="false" Precision="7" />
                  </EntityType>
                  <EntityContainer Name="TestContext">
                      <EntitySet Name="Schedules" EntityType="Self.Schedule" />
                  </EntityContainer>
              </Schema>
          </edmx:ConceptualModels>
          <edmx:Mappings>
              <Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2009/11/mapping/cs">
                  <EntityContainerMapping StorageEntityContainer="TestModelStoreContainer" CdmEntityContainer="TestContext">
                      <EntitySetMapping Name="Schedules">
                          <EntityTypeMapping TypeName="TestModel.Schedule">
                              <MappingFragment StoreEntitySet="Schedules">
                                  <ScalarProperty Name="Id" ColumnName="Id" />
                                  <ScalarProperty Name="StartTime" ColumnName="StartTime" />
                              </MappingFragment>
                          </EntityTypeMapping>
                      </EntitySetMapping>
                  </EntityContainerMapping>
              </Mapping>
          </edmx:Mappings>
      </edmx:Runtime>
  </edmx:Edmx>
  """;

            // Act
            var loader = new EdmxLoader();
           loader.Load(edmx);

            // Assert
            loader.EdmxSchemaErrors.Should().BeEmpty();
            loader.Entities.Should().HaveCount(1);
            
            var scheduleEntity = loader.Entities.First();
            scheduleEntity.EntityType.Name.Should().Be("Schedule");
            
            var startTimeProperty = scheduleEntity.EntityType.Properties.FirstOrDefault(p => p.Name == "StartTime");
            startTimeProperty.Should().NotBeNull();
            // The property should be recognized as TimeOnly type
        }

        [TestMethod]
        public void CanProcessDateOnlyTimeOnlyMappings()
        {
            // Arrange
            var edmx = """
                <?xml version="1.0" encoding="utf-8"?>
                <edmx:Edmx Version="3.0" xmlns:edmx="http://schemas.microsoft.com/ado/2009/11/edmx">
                    <edmx:Runtime>
                        <edmx:StorageModels>
                            <Schema Namespace="TestModel.Store" Provider="System.Data.SqlClient" ProviderManifestToken="2012" xmlns="http://schemas.microsoft.com/ado/2009/11/edm/ssdl">
                                <EntityType Name="Appointments">
                                    <Key>
                                        <PropertyRef Name="Id" />
                                    </Key>
                                    <Property Name="Id" Type="int" Nullable="false" />
                                    <Property Name="AppointmentDate" Type="date" Nullable="false" />
                                    <Property Name="AppointmentTime" Type="time" Nullable="false" />
                                    <Property Name="CreatedDateTime" Type="datetime2" Nullable="false" />
                                </EntityType>
                                <EntityContainer Name="TestModelStoreContainer">
                                    <EntitySet Name="Appointments" EntityType="TestModel.Store.Appointments" />
                                </EntityContainer>
                            </Schema>
                        </edmx:StorageModels>
                        <edmx:ConceptualModels>
                            <Schema Namespace="TestModel" Alias="Self" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
                                <EntityType Name="Appointment">
                                    <Key>
                                        <PropertyRef Name="Id" />
                                    </Key>
                                    <Property Name="Id" Type="Int32" Nullable="false" />
                                    <Property Name="AppointmentDate" Type="DateOnly" Nullable="false" />
                                    <Property Name="AppointmentTime" Type="TimeOnly" Nullable="false" />
                                    <Property Name="CreatedDateTime" Type="DateTime" Nullable="false" />
                                </EntityType>
                                <EntityContainer Name="TestContext">
                                    <EntitySet Name="Appointments" EntityType="Self.Appointment" />
                                </EntityContainer>
                            </Schema>
                        </edmx:ConceptualModels>
                        <edmx:Mappings>
                            <Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2009/11/mapping/cs">
                                <EntityContainerMapping StorageEntityContainer="TestModelStoreContainer" CdmEntityContainer="TestContext">
                                    <EntitySetMapping Name="Appointments">
                                        <EntityTypeMapping TypeName="TestModel.Appointment">
                                            <MappingFragment StoreEntitySet="Appointments">
                                                <ScalarProperty Name="Id" ColumnName="Id" />
                                                <ScalarProperty Name="AppointmentDate" ColumnName="AppointmentDate" />
                                                <ScalarProperty Name="AppointmentTime" ColumnName="AppointmentTime" />
                                                <ScalarProperty Name="CreatedDateTime" ColumnName="CreatedDateTime" />
                                            </MappingFragment>
                                        </EntityTypeMapping>
                                    </EntitySetMapping>
                                </EntityContainerMapping>
                            </Mapping>
                        </edmx:Mappings>
                    </edmx:Runtime>
                </edmx:Edmx>
                """;

            // Act
            var loader = new EdmxLoader();
            loader.Load(edmx);

            // Assert
            loader.EdmxSchemaErrors.Should().BeEmpty();
            loader.Entities.Should().HaveCount(1);
            
            var appointmentEntity = loader.Entities.First();
            appointmentEntity.EntityType.Name.Should().Be("Appointment");
            appointmentEntity.EntityType.Properties.Should().HaveCount(4);
            
            // Verify all date/time types are present
            appointmentEntity.EntityType.Properties.Should().Contain(p => p.Name == "AppointmentDate");
            appointmentEntity.EntityType.Properties.Should().Contain(p => p.Name == "AppointmentTime");
            appointmentEntity.EntityType.Properties.Should().Contain(p => p.Name == "CreatedDateTime");
        }

    }

}
