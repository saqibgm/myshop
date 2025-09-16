using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Reports
{
    /// <summary>
    /// Represents a country report search model
    /// </summary>
    public partial class ProfitReportSearchModel : BaseSearchModel
    {
        #region Ctor

        public ProfitReportSearchModel()
        {
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailableCustomers = new List<SelectListItem>();
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
        public DateTime? EndDate { get; set; } = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day);
        public bool InvoiceAmountEnabled { get; set; } = true;
        [NopResourceDisplayName("Admin.Reports.SalesmanCommission.InvoiceAmount")]
        public string InvoiceAmount { get; set; }

        public bool SearchKeywordEnabled { get; set; } = true;
        [NopResourceDisplayName("Admin.Reports.SalesmanCommission.SearchKeyword")]       
        public string SearchKeyword { get; set; }

        public bool InvoiceNoEnabled { get; set; } = true;
        [NopResourceDisplayName("Admin.Reports.ProfitReport.Fields.InvoiceNo")]
        public string InvoiceNo { get; set; }



       

        [NopResourceDisplayName("Admin.Reports.Sales.Country.OrderStatus")]
        public int OrderStatusId { get; set; }

        [NopResourceDisplayName("Admin.Reports.ProfitReport.Fields.CustomerName")]
        public int CustomerId { get; set; }
        public int SalesmanId { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }

        public IList<SelectListItem> AvailableCustomers { get; set; }

        #endregion
    }
}