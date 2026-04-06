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
    public partial class ProductStatusType : DbObservableObject, IDbStatusEnum, ICreatedAuditable, IUpdatedAuditable, IUpdaterTrackable<Guid>
    {
    
        #region Private Members
        
        private Nullable<Guid> _createdById;
        private DateTimeOffset _dateCreated;
        private Nullable<DateTimeOffset> _dateUpdated;
        private string _displayName;
        private Guid _id;
        private bool _isActive;
        private int _sortOrder;
        private Nullable<Guid> _updatedById;
        private ObservableCollection<Product> _products;
        
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
        [StringLength(50)]
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
        public bool IsActive
        {
            get => _isActive;
            set => Set(() => IsActive, ref _isActive, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public Nullable<Guid> CreatedById
        {
            get => _createdById;
            set => Set(() => CreatedById, ref _createdById, value);
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
        public Nullable<Guid> UpdatedById
        {
            get => _updatedById;
            set => Set(() => UpdatedById, ref _updatedById, value);
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
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => Set(() => Products, ref _products, value);
        }
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// 
        /// </summary>
        public ProductStatusType()
        {
        }
        
        #endregion
        
    }
    
}
