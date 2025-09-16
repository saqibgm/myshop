using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Areas.Admin.Models.Reports;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class ReportController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IReportModelFactory _reportModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public ReportController(
            IPermissionService permissionService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            IReportModelFactory reportModelFactory)
        {
            _permissionService = permissionService;
            _reportModelFactory = reportModelFactory;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Methods

        #region Low stock

        public virtual IActionResult LowStock()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = _reportModelFactory.PrepareLowStockProductSearchModel(new LowStockProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult LowStockList(LowStockProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _reportModelFactory.PrepareLowStockProductListModel(searchModel);

            return Json(model);
        }

        #endregion

        #region Bestsellers

        public virtual IActionResult Bestsellers()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //prepare model
            var model = _reportModelFactory.PrepareBestsellerSearchModel(new BestsellerSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult BestsellersList(BestsellerSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _reportModelFactory.PrepareBestsellerListModel(searchModel);

            return Json(model);
        }

        #endregion

        #region Never Sold

        public virtual IActionResult NeverSold()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //prepare model
            var model = _reportModelFactory.PrepareNeverSoldSearchModel(new NeverSoldReportSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult NeverSoldList(NeverSoldReportSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _reportModelFactory.PrepareNeverSoldListModel(searchModel);

            return Json(model);
        }

        #endregion      

        #region Country sales

        public virtual IActionResult CountrySales()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.OrderCountryReport))
                return AccessDeniedView();

            //prepare model
            var model = _reportModelFactory.PrepareCountrySalesSearchModel(new CountryReportSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult CountrySalesList(CountryReportSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.OrderCountryReport))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _reportModelFactory.PrepareCountrySalesListModel(searchModel);

            return Json(model);
        }

        #endregion

        #region Customer reports

        public virtual IActionResult RegisteredCustomers()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prepare model
            var model = _reportModelFactory.PrepareCustomerReportsSearchModel(new CustomerReportsSearchModel());

            return View(model);
        }

        public virtual IActionResult BestCustomersByOrderTotal()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prepare model
            var model = _reportModelFactory.PrepareCustomerReportsSearchModel(new CustomerReportsSearchModel());

            return View(model);
        }

        public virtual IActionResult BestCustomersByNumberOfOrders()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prepare model
            var model = _reportModelFactory.PrepareCustomerReportsSearchModel(new CustomerReportsSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ReportBestCustomersByOrderTotalList(BestCustomersReportSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _reportModelFactory.PrepareBestCustomersReportListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult ReportBestCustomersByNumberOfOrdersList(BestCustomersReportSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _reportModelFactory.PrepareBestCustomersReportListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult ReportRegisteredCustomersList(RegisteredCustomersReportSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _reportModelFactory.PrepareRegisteredCustomersReportListModel(searchModel);

            return Json(model);
        }

        #endregion

        #endregion
        #region Saleman Commission

        public virtual IActionResult SalemanCommission()
        {
            //prepare model
            var model = _reportModelFactory.PrepareSalemanCommissionSearchModel(new SalemanCommissionReportSearchModel());
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult SalemanCommissionList(SalemanCommissionReportSearchModel searchModel)
        {
            //if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
            //    return AccessDeniedDataTablesJson();

            //prepare model
            if (!_workContext.CurrentCustomer.IsAdmin())
            {
                var firstName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.FirstNameAttribute);
                var lastName = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, NopCustomerDefaults.LastNameAttribute);
                
                searchModel.SearchKeyword = firstName + " " + lastName;
            }
            searchModel.SearchKeyword = searchModel.SearchKeyword?.ToLower();
            var model = _reportModelFactory.PrepareSalemanCommissionListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult CustomerProfileStaticsList(CustomerProfileSearchModel SearchModel)
        {
            //******************************** CROSS MODULE CONTROLLER**********************************/
            //prepare model
            var model = _reportModelFactory.PrepareSalemanStatisticsList(SearchModel);
            //******************************** JSON CONVERSION**********************************/
            return Json(model);
        }

        public virtual IActionResult ProfitReport()
        {
            var searchModel = _reportModelFactory.PrepareProfitSearchModel(new ProfitReportSearchModel());
            return View(searchModel);
        }
        [HttpPost]
        public virtual IActionResult ProfitReportList(ProfitReportSearchModel searchModel)
        {
            //prepare model
            var model = _reportModelFactory.PrepareProfitListModel(searchModel);

            return Json(model);
        }
        #endregion

    }
}


