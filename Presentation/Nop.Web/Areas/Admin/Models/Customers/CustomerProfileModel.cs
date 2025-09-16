using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerProfileModel : BaseNopEntityModel
    {
        public int? UniqueCustomers { get; set; }

        public decimal TotalSale { get; set; }
        public string TotalSaleStr { get; set; }

        public int? Order { get; set; }

        public decimal Commission { get; set; }

        public string CommissionStr { get; set; }
        public decimal Profit { get; set; }

        public string ProfitStr { get; set; }
        public decimal UnpaidAmount { get; set; }

        public string MonthName { get; set; }


    }
    
}
