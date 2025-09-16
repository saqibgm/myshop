using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using static Nop.Web.Areas.Admin.Models.Customers.CustomerModel;

namespace Nop.Web.Areas.Admin.Models.Reports
{
    /// <summary>
    /// Represents a country report search model
    /// </summary>
    public partial class SalemanCommissionReportSearchModel : BaseSearchModel
    {
        #region Ctor

        public SalemanCommissionReportSearchModel()
        {
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
        }

        #endregion

        #region Properties
        [NopResourceDisplayName("Admin.Orders.List.Product")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.Reports.Sales.Country.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; } = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);

        [NopResourceDisplayName("Admin.Reports.Sales.Country.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; } = System.DateTime.UtcNow;
        public bool InvoiceAmountEnabled { get; set; } = true;
        [NopResourceDisplayName("Admin.Reports.SalesmanCommission.InvoiceAmount")]
        public string InvoiceAmount { get; set; }

        public bool SearchKeywordEnabled { get; set; } = true;
        [NopResourceDisplayName("Admin.Reports.SalemanCommission.Fields.Saleman")]       
        public int? SalemanId { get; set; }    
        public string SearchKeyword { get; set; }    

        [NopResourceDisplayName("Admin.Reports.Sales.Country.OrderStatus")]
        public int OrderStatusId { get; set; }

        [NopResourceDisplayName("Admin.Reports.Sales.Country.PaymentStatus")]
        public int PaymentStatusId { get; set; }
        // Saleman
        public IList<CustomerSalemanListItem> SalemanList { get; set; }
        public IList<SelectListItem> AvailableOrderStatuses { get; set; }

        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }

        #endregion
    }
}