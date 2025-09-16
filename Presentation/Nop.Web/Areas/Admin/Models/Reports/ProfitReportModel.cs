using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Reports
{
    /// <summary>
    /// Represents a country report model, By Blazor Technologies Inc, Development Team, wwww.blazortech.com
    /// </summary>
    public partial class ProfitReportModel : BaseNopModel
    {
        #region Properties

      
        public string CustomerName { get; set; }
   
        public int InvoiceCount { get; set; }

     
        public string TotalSaleStr { get; set; }

       
        public decimal TotalSale { get; set; }

     
        public string TotalAmountStr { get; set; }

    
        public decimal TotalAmount { get; set; }
     
        public decimal Profit { get; set; }
      
        public string ProfitStr { get; set; }
       
        public decimal CommissionAmmount { get; set; }
        public string CommissionAmmountStr { get; set; }

        public string TotalCommission { get; set; }


        #endregion
    }
}