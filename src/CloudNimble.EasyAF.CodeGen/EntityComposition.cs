using CloudNimble.EasyAF.CodeGen.Legacy;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace CloudNimble.EasyAF.CodeGen
{

    /// <summary>
    /// 
    /// </summary>
    public class EntityComposition
    {

        #region Private Members

        private const string CreatedById = "CreatedById";
        private const string DateCreated = "DateCreated";
        private const string DateUpdated = "DateUpdated";
        private const string DisplayName = "DisplayName";
        private const string Id = "Id";
        private const string InstructionText = "InstructionText";
        private const string IsActive = "IsActive";
        private const string PrimaryTargetDisplayText = "PrimaryTargetDisplayText";
        private const string PrimaryTargetSortOrder = "PrimaryTargetSortOrder";
        private const string SecondaryTargetDisplayText = "SecondaryTargetDisplayText";
        private const string SecondaryTargetSortOrder = "SecondaryTargetSortOrder";
        private const string SortOrder = "SortOrder";
        private const string StateTypeId = "StateTypeId";
        private const string StatusTypeId = "StatusTypeId";
        private const string UpdatedById = "UpdatedById";

        private readonly List<string> _trackedProperties = new List<string>
        {
            CreatedById,
            DateCreated,
            DateUpdated,
            DisplayName,
            Id,
            InstructionText,
            IsActive,
            PrimaryTargetDisplayText,
            PrimaryTargetSortOrder,
            SecondaryTargetDisplayText,
            SecondaryTargetSortOrder,
            SortOrder,
            StateTypeId,
            StatusTypeId,
            UpdatedById
        };

        private readonly List<string> _stateMachineProperties = new List<string>
        {
            DisplayName,
            Id,
            InstructionText,
            IsActive,
            PrimaryTargetDisplayText,
            PrimaryTargetSortOrder,
            SecondaryTargetDisplayText,
            SecondaryTargetSortOrder,
            SortOrder,
        };

        #endregion

        #region Properties

        /// <summary>
        /// A <see cref="List{NavigationProperty}"/> containing all of the Entity properties that map to the Many side of a One to Many association.
        /// </summary>
        public List<NavigationProperty> CollectionNavigationProperties { get; private set; }

        /// <summary>
        /// A <see cref="List{NavigationProperty}"/> containing all of the Entity properties that are not .NET simple types (int, string, etc).
        /// </summary>
        public List<EdmProperty> ComplexProperties { get; private set; }

        /// <summary>
        /// The Entity Framework <see cref="EntityType"/> that represents the EF-processed shape and structure of the Entity.
        /// </summary>
        public EntityType EntityType { get; set; }

        /// <summary>
        /// A boolean specifying whether or not this Entity has StateType and StateTypeId properties.
        /// </summary>
        public bool HasState { get; private set; }

        /// <summary>
        /// A boolean specifying whether or not this Entity has StatusType and StatusTypeId properties.
        /// </summary>
        public bool HasStatus { get; private set; }

        /// <summary>
        /// A boolean specifying whether or not this Entity has an IsActive property.
        /// </summary>
        public bool IsActiveTrackable { get; private set; }

        /// <summary>
        /// A boolean specifying whether or not this Entity has a DateCreated property.
        /// </summary>
        public bool IsCreatedAuditable { get; private set; }

        /// <summary>
        /// A boolean specifying whether or not this Entity has a CreatedById property.
        /// </summary>
        public bool IsCreatorTrackable { get; private set; }

        /// <summary>
        /// A boolean specifying whether or not this Entity has Id, DisplayName, and IsActive properties.
        /// </summary>
        public bool IsDbEnum { get; private set; }

        /// <summary>
        /// A boolean specifying whether or not <see cref="IsDbEnum"/> is true and the Entity has InstructionText, PrimaryTargetDisplayText, 
        /// PrimaryTargetSortOrder, SecondaryTargetDisplayText, SecondaryTargetSortOrder properties.
        /// </summary>
        public bool IsDbStateEnum { get; private set; }

        /// <summary>
        /// A boolean specifying whether or not <see cref="IsDbEnum"/> is true and the EntityName ends in "StatusType".
        /// </summary>
        public bool IsDbStatusEnum { get; private set; }

        /// <summary>
        /// A boolean specifying whether or not this Entity has DisplayName property.
        /// </summary>
        public bool IsHumanReadable { get; private set; }

        /// <summary>
        /// A boolean specifying whether or not this Entity has an Id property.
        /// </summary>
        public bool IsIdentifiable { get; private set; }

        /// <summary>
        /// A boolean specifying whether or not this Entity has a SortOrder property.
        /// </summary>
        public bool IsSortable { get; private set; }

        /// <summary>
        /// A boolean specifying whether or not this Entity has an DateUpdated property.
        /// </summary>
        public bool IsUpdatedAuditable { get; private set; }

        /// <summary>
        /// A boolean specifying whether or not this Entity has a UpdatedById property.
        /// </summary>
        public bool IsUpdaterTrackable { get; private set; }

        /// <summary>
        /// A <see cref="List{EdmProperty}"/> containing all of the Entity properties that make up the Entity's keys.
        /// </summary>
        public List<EdmProperty> KeyProperties { get; private set; }

        /// <summary>
        /// A <see cref="List{NavigationProperty}"/> containing all of the Entity properties that map to the other end of a One to One association.
        /// </summary>
        public List<NavigationProperty> NavigationProperties { get; private set; }

        /// <summary>
        /// A <see cref="List{EdmProperty}"/> containing all of the Entity properties that are NOT tracked by EasyAF.
        /// </summary>
        public List<EdmProperty> OtherProperties { get; private set; }

        /// <summary>
        /// A <see cref="List{EdmProperty}"/> containing all of the Entity properties that make up the Entity's keys.
        /// </summary>
        public List<EdmProperty> PropertiesWithDefaults { get; private set; }

        /// <summary>
        /// A <see cref="List{NavigationProperty}"/> containing all of the Entity properties that are .NET simple types (int, string, etc).
        /// </summary>
        public List<EdmProperty> SimpleProperties { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public EntityComposition(EntityType entity)
        {
            EntityType = entity ?? throw new ArgumentNullException(nameof(entity));

            KeyProperties = entity.Properties.Where(c => c.TypeUsage.EdmType is SimpleType && c.DeclaringType == entity && MetadataTools.IsKey(c)).ToList();
            SimpleProperties = entity.Properties.Where(c => c.TypeUsage.EdmType is SimpleType && c.DeclaringType == entity && c.Name != "GeoCode").ToList();
            ComplexProperties = entity.Properties.Where(c => c.TypeUsage.EdmType is ComplexType && c.DeclaringType == entity && c.Name != "GeoCode").ToList();
            NavigationProperties = entity.NavigationProperties.Where(np => np.DeclaringType == entity).ToList();
            PropertiesWithDefaults = entity.Properties.Where(c => c.TypeUsage.EdmType is SimpleType && c.DeclaringType == entity && c.DefaultValue is not null).ToList();
            CollectionNavigationProperties = entity.NavigationProperties.Where(np => np.DeclaringType == entity && np.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many).ToList();
            OtherProperties = entity.Properties.Where(c => !_trackedProperties.Contains(c.Name)).ToList();

            HasState = entity.Properties.Any(c => c.Name == StateTypeId);
            HasStatus = entity.Properties.Any(c => c.Name == StatusTypeId);
            IsActiveTrackable = entity.Properties.Any(c => c.Name == IsActive && c.TypeName.ToLower() == "boolean");
            IsCreatedAuditable = entity.Properties.Any(c => c.Name == DateCreated && c.TypeName.ToLower().Contains("datetime"));
            IsCreatorTrackable = entity.Properties.Any(c => c.Name == CreatedById);
            IsDbStateEnum = _stateMachineProperties.All(c => entity.Properties.Any(d => d.Name == c));
            IsDbStatusEnum = entity.Name.EndsWith("StatusType");
            IsHumanReadable = entity.Properties.Any(c => c.Name == DisplayName);
            IsIdentifiable = entity.Properties.Any(c => c.Name == Id && MetadataTools.IsKey(c));
            IsSortable = entity.Properties.Any(c => c.Name == SortOrder);
            IsUpdatedAuditable = entity.Properties.Any(c => c.Name == DateUpdated && c.TypeName.ToLower().Contains("datetime"));
            IsUpdaterTrackable = entity.Properties.Any(c => c.Name == UpdatedById);

            // RWM: Can't set this one until the others are computed.
            IsDbEnum = IsIdentifiable && IsActiveTrackable && IsHumanReadable && IsSortable;
        }

        #endregion

    }

}
