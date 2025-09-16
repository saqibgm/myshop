using System;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.ShoppingCart
{
    /// <summary>
    /// Represents a shopping cart item model
    /// </summary>
    public partial class ShoppingCartItemModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.CurrentCarts.Store")]
        public string Store { get; set; }

        [NopResourceDisplayName("Admin.CurrentCarts.Product")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.CurrentCarts.Product")]
        public string ProductName { get; set; }

        public string AttributeInfo { get; set; }

        [NopResourceDisplayName("Admin.CurrentCarts.UnitPrice")]
        public string UnitPrice { get; set; }

        [NopResourceDisplayName("Admin.CurrentCarts.Quantity")]
        public int Quantity { get; set; }
        [NopResourceDisplayName("Admin.CurrentCarts.StockQuantity")]
        //categories
        public int StockQuantity { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.categories")]
        public string CategoriesText { get; set; }
        //categories
        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Categories")]
        public IList<int> SelectedCategoryIds { get; set; }
        public string Sku { get; set; }

        [NopResourceDisplayName("Admin.CurrentCarts.Total")]
        public string Total { get; set; }

        [NopResourceDisplayName("Admin.CurrentCarts.UpdatedOn")]
        public DateTime UpdatedOn { get; set; }
        [NotMapped]
        public decimal UnitPriceDec { get; set; }

        #endregion
    }
}