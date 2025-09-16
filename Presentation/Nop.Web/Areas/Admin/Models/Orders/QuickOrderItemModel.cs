using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    /// <summary>
    /// Represents an order item model
    /// </summary>
    public partial class QuickOrderItemModel : BaseNopEntityModel
    {
        
        #region Properties

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int? NewStockComing { get; set; } 
        public string Sku { get; set; } 
        public decimal UnitPriceExclTax { get; set; }

        public decimal UnitPriceInclTaxValue { get; set; }
        public decimal MinPrice { get; set; }
        public decimal OldPrice { get; set; }
        public decimal LastPrice { get; set; }
        public decimal PurchasePrice { get; set; }

        public int Quantity { get; set; }
        public int StockQuantity { get; set; }

        public decimal DiscountInclTax { get; set; }
        public decimal DiscountExclTax { get; set; }
        public decimal TotalExclTax { get; set; } = new decimal(0.0);
        public decimal TotalInclTax { get; set; } = new decimal(0.0);

        #endregion
    }
}