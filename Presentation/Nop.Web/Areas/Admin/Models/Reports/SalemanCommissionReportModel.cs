using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Reports
{
    /// <summary>
    /// Represents a country report model
    /// </summary>
    public partial class SalemanCommissionReportModel : BaseNopModel
    {
        #region Properties
        public int SalemanId { get; set; }
        [NopResourceDisplayName("Admin.Reports.SalemanCommission.Fields.Saleman")]
        public string SalemanName { get; set; }

        [NopResourceDisplayName("Admin.Reports.SalemanCommission.Fields.Date")]
        public int Date { get; set; }

        [NopResourceDisplayName("Admin.Reports.SalemanCommission.Fields.Order")]
        public int TotalOrders { get; set; }
        [NopResourceDisplayName("Admin.Reports.SalemanCommission.Fields.SaleAmount")]
        public string TotalSaleAmount { get; set; }

        [NopResourceDisplayName("Admin.Reports.SalemanCommission.Fields.CommissionPercentage")]
        public decimal CommissionPercentage { get; set; }

        [NopResourceDisplayName("Admin.Reports.SalemanCommission.Fields.CommissionAmount")]
        public string CommissionAmount { get; set; }

        [NopResourceDisplayName("Admin.Reports.SalemanCommission.Fields.TotalSalemanSalaryIncCommission")]
        public string TotalSalemanSalaryIncCommission { get; set; }
        [NopResourceDisplayName("Admin.Reports.SalemanCommission.Fields.TotalProfit")]
        public string TotalProfit { get; set; }

        #endregion
    }
}