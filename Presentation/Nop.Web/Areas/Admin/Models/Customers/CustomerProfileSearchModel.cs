using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerProfileSearchModel : BaseSearchModel
    {

        #region "Common Page"       
        #endregion
        #region Saleman 
        public string MonthName { get; set; }
        [NopResourceDisplayName("admin.customers.csutomerprofile.UniqueCustomers")]
        public string SID { get; set; }
        public int SalemanId { get; set; }
        public string Avator { get; set; }

        [NopResourceDisplayName("TotalSale")]
        public string SurName { get; set; }

        [NopResourceDisplayName("admin.customers.csutomerprofile.Orders")]
        public string Email { get; set; }

        [NopResourceDisplayName("admin.customers.csutomerprofile.CommissionPercentage")]
        public string Phone { get; set; }

        [NopResourceDisplayName("admin.customers.csutomerprofile.Profit")]
        public string Mobile { get; set; }
        public DateTime DOJ { get; set; }
        public string BasicSalary { get; set; }
        public string CommissionPercentage { get; set; }
        public string CommissionMinTargert { get; set; }
        [NopResourceDisplayName("admin.customers.csutomerprofile.UnpaidAmount")]
        public string RoleName { get; set; }
        #endregion

        #region Manager       

        [NopResourceDisplayName("admin.customers.csutomerprofile.UniqueCustomers")]
        public string SIDMngr { get; set; }
        public string AvatorMngr { get; set; }
        [NopResourceDisplayName("TotalSale")]
        public string SurNameMngr { get; set; }

        [NopResourceDisplayName("admin.customers.csutomerprofile.Orders")]
        public string EmailMngr { get; set; }

        [NopResourceDisplayName("admin.customers.csutomerprofile.CommissionPercentage")]
        public string PhoneMngr { get; set; }

        [NopResourceDisplayName("admin.customers.csutomerprofile.Profit")]
        public string MobileMngr { get; set; }
        public DateTime DOJMngr { get; set; }

        public string RoleNameMngr { get; set; }
        #endregion
    }
}
