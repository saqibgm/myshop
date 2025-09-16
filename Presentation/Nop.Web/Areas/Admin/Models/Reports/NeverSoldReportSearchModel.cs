using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace Nop.Web.Areas.Admin.Models.Reports
{
    /// <summary>
    /// Represents a never sold products report search model
    /// </summary>
    public partial class NeverSoldReportSearchModel : BaseSearchModel
    {
        #region Ctor

        public NeverSoldReportSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Reports.Sales.NeverSold.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; } = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);

        [NopResourceDisplayName("Admin.Reports.Sales.NeverSold.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; } = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day);

        [NopResourceDisplayName("Admin.Reports.Sales.NeverSold.SearchCategory")]
        public int SearchCategoryId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        [NopResourceDisplayName("Admin.Reports.Sales.NeverSold.SearchManufacturer")]
        public int SearchManufacturerId { get; set; }

        public IList<SelectListItem> AvailableManufacturers { get; set; }

        [NopResourceDisplayName("Admin.Reports.Sales.NeverSold.SearchStore")]
        public int SearchStoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
        
        [NopResourceDisplayName("Admin.Reports.Sales.NeverSold.SearchVendor")]
        public int SearchVendorId { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }

        public bool IsLoggedInAsVendor { get; set; }

        #endregion
    }
}