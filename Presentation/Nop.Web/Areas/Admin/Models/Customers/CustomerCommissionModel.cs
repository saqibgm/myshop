using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nop.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer model
    /// </summary>
    public partial class CustomerCommissionModel : BaseNopEntityModel
    {
        #region Ctor

        public CustomerCommissionModel()
        {

        }

        #endregion

        #region Properties
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.CustomerId")]
        public int CustomerId { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.CommissionMonth")]
        public DateTime CommissionMonth { get; set; }
        public string CommissionMonthStr { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.BasicSalary")]
        public decimal BasicSalary { get; set; }
        public string BasicSalaryStr { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.CommissionPercentage")]
        public decimal CommissionPercentage { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.CommissionMinTargert")]
        public decimal CommissionMinTargert { get; set; }
        public string CommissionMinTargertStr { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.TotalOrders")]
        public int TotalOrders { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.TotalSale")]
        public decimal TotalSale { get; set; }
        public string TotalSaleStr { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.TotalProfit")]
        public decimal TotalProfit { get; set; }
        public string TotalProfitStr { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.TotalCommission")]
        public decimal TotalCommission { get; set; }
        public string TotalCommissionStr { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.TotalSalary")]
        public decimal TotalSalary { get; set; }
        public string TotalSalaryStr { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.LastActivityDateUtc")]
        public DateTime LastActivityDateUtc { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.Active")]
        public bool Active { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerCommission.Fields.Deleted")]
        public bool Deleted { get; set; }
        

        public CustomerModel CustomerModel { get; set; }

        #endregion

        #region Nested classes
        
        #endregion
    }
}