using EasyAFModel.Core;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Restier.Core.Model;

namespace EasyAFModel
{

    public partial class EasyAFEntitiesModelBuilder
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
#pragma warning disable CA1822 // Mark members as static
        partial void ExtendModel(ODataModelBuilder modelBuilder)
#pragma warning restore CA1822 // Mark members as static
        {

            modelBuilder.EntitySet<Inquiry>("Inquiries")
                .IgnoreAuditFields();

            modelBuilder.EntitySet<InquiryStateType>("InquiryStateTypes")
                .IgnoreAuditFields();

            modelBuilder.EntitySet<Product>("Products")
                .IgnoreAuditFields();

            modelBuilder.EntitySet<ProductStatusType>("ProductStatusTypes")
                .IgnoreAuditFields();

            modelBuilder.EntitySet<User>("Users")
                .IgnoreAuditFields();

        }

    }

}
