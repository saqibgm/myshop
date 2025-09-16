using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer orders search model
    /// </summary>
    public partial class CustomerCommissionSearchModel : BaseSearchModel
    {
        #region Properties

        public int SalemanId { get; set; }

        #endregion
    }
}