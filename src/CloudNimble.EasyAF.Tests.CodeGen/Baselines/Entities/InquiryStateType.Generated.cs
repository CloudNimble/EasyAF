using CloudNimble.EasyAF.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyAFModel
{

    /// <summary>
    /// 
    /// </summary>
    public partial class InquiryStateType : DbObservableObject, IDbStateEnum, ICreatedAuditable, ICreatorTrackable<Guid>, IUpdatedAuditable, IUpdaterTrackable<Guid>
    {
    
        #region Private Members
        
        private Guid _createdById;
        private DateTimeOffset _dateCreated;
        private Nullable<DateTimeOffset> _dateUpdated;
        private string _displayName;
        private Guid _id;
        private string _instructionText;
        private bool _isActive;
        private string _primaryTargetDisplayText;
        private int _primaryTargetSortOrder;
        private string _secondaryTargetDisplayText;
        private int _secondaryTargetSortOrder;
        private int _sortOrder;
        private Nullable<Guid> _updatedById;
        private ObservableCollection<Inquiry> _inquiries;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// 
        /// </summary>
        public Guid Id
        {
            get => _id;
            set => Set(() => Id, ref _id, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        [StringLength(30)]
        public string DisplayName
        {
            get => _displayName;
            set => Set(() => DisplayName, ref _displayName, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public int SortOrder
        {
            get => _sortOrder;
            set => Set(() => SortOrder, ref _sortOrder, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        [StringLength(250)]
        public string InstructionText
        {
            get => _instructionText;
            set => Set(() => InstructionText, ref _instructionText, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public int PrimaryTargetSortOrder
        {
            get => _primaryTargetSortOrder;
            set => Set(() => PrimaryTargetSortOrder, ref _primaryTargetSortOrder, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        [StringLength(50)]
        public string PrimaryTargetDisplayText
        {
            get => _primaryTargetDisplayText;
            set => Set(() => PrimaryTargetDisplayText, ref _primaryTargetDisplayText, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public int SecondaryTargetSortOrder
        {
            get => _secondaryTargetSortOrder;
            set => Set(() => SecondaryTargetSortOrder, ref _secondaryTargetSortOrder, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        [StringLength(50)]
        public string SecondaryTargetDisplayText
        {
            get => _secondaryTargetDisplayText;
            set => Set(() => SecondaryTargetDisplayText, ref _secondaryTargetDisplayText, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => Set(() => IsActive, ref _isActive, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public Guid CreatedById
        {
            get => _createdById;
            set => Set(() => CreatedById, ref _createdById, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public Nullable<Guid> UpdatedById
        {
            get => _updatedById;
            set => Set(() => UpdatedById, ref _updatedById, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset DateCreated
        {
            get => _dateCreated;
            set => Set(() => DateCreated, ref _dateCreated, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public Nullable<DateTimeOffset> DateUpdated
        {
            get => _dateUpdated;
            set => Set(() => DateUpdated, ref _dateUpdated, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<Inquiry> Inquiries
        {
            get => _inquiries;
            set => Set(() => Inquiries, ref _inquiries, value);
        }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// 
        /// </summary>
        public InquiryStateType()
        {
        }
        
        #endregion
        
    }
    
}
