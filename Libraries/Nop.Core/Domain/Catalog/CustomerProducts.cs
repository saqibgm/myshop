using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product
    /// </summary>
    public partial class CustomerProducts : BaseEntity
    {
        public CustomerProducts()
        {
            ProductList = new List<CustomerProductDetailsModel>();
        }

        public IList<CustomerProductDetailsModel> ProductList { get; set; }

        public partial class CustomerProductDetailsModel : BaseEntity
        {
            public string ProductName { get; set; }
            public string Sku { get; set; }
            public string ManufacturerPartNumber { get; set; }
            public string VendorName { get; set; }
            public int Quantity { get; set; }
            public int OrderCount { get; set; }
            public int TotalAmount { get; set; }
            public DateTime LastOrderOn { get; set; }
            public decimal LastPrice { get; set; }
            public string PictureURL  { get; set; }

        }

    }


}