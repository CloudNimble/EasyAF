using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EasyAFModel.Managers
{

    public partial class ProductManager
    {

        partial void OnInsertingInternal(Product entity)
        {
            if (entity.StatusType is null || entity.StatusTypeId == Guid.Empty)
            {
                entity.StatusTypeId = DataContext.ProductStatusTypes.Where(c => c.SortOrder == 0).FirstOrDefault()?.Id ?? Guid.Empty;
            }
        }

        #region Public Methods

        /// <summary>
        /// Update all <see cref="Product"/> entries with the specified StatusTypeId.
        /// </summary>
        /// <param name="statusTypeId">Identifier for records to be updated.</param>
        /// <param name="updateExpression">Update expression.</param>
        /// <returns></returns>
        public async Task<int> UpdateByStatusType(Guid statusTypeId, Expression<Func<Product, Product>> updateExpression)
        {
            return await DirectUpdateAsync(c => c.StatusTypeId == statusTypeId, updateExpression);
        }

        /// <summary>
        /// Delete all <see cref="Product"/> entries with the specified StatusTypeId.
        /// </summary>
        /// <param name="statusTypeId">Identifier for records to be updated.</param>
        /// <returns></returns>
        public async Task<int> DeleteByStatusType(Guid statusTypeId)
        {
            return await DirectDeleteAsync(c => c.StatusTypeId == statusTypeId);
        }

        #endregion

    }

}
