using System;
using System.Collections.Generic;
using Nop.Core.Domain.Orders;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using static Nop.Web.Models.Catalog.ProductDetailsModel;

namespace Nop.Web.Models.Order
{
    public partial class CustomerProductListModel : BaseNopModel
    {
        public CustomerProductListModel()
        {
            Orders = new List<OrderDetailsModel>();
        }

        public IList<OrderDetailsModel> Orders { get; set; }

        public partial class ProductDetailsModel : BaseNopEntityModel
        {
            public string ProductName { get; set; }
            public string Sku { get; set; }
            public string ManufacturerPartNumber { get; set; }
            public string VendorName { get; set; }
            public int Count { get; set; }
            public int Quantity { get; set; }
            public int TotalAmount { get; set; }
            public int LastOrderOn { get; set; }
            public int LastPrice { get; set; }
            public PictureModel DefaultPictureModel { get; set; }

        }

    }
}