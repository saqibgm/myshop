using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Common;
using Nop.Services.Weclapp.model;
using System.Dynamic;
using Nop.Core.Domain.Logging;
namespace Nop.Services.Weclapp
{
    public partial interface IWeclappIntegrationService
    {
        //bool SendProductsToWeclapp(List<WProductModel> products);
        Result SaveProductToWeclapp(IList<Product> products);
        Result SaveProductToWeclapp(IList<Core.Domain.Catalog.Product> products, ExportBatch exportBatch);
        Result SaveProductToWeclapp(Core.Domain.Catalog.Product product, int batchId);
        string CreateProductInWeclapp(WProduct product);
        bool UpdateProductInWeclapp(WProduct product);
        WProduct GetWeclappProduct(Product product);


        Result SaveCustomerToWeclapp(IList<Customer> customers);
        Result SaveCustomerToWeclapp(Customer customer, int batchId);

        string CreateCustomerInWeclapp(WCustomer customer);
        bool UpdateCustomerInWeclapp(WCustomer customer);
        WCustomer GetWeclappCustomer(Customer customer);

        Result SaveOrderToWeclapp(IList<Order> orders);
        string CreateOrderInWeclapp(WOrder order);
        bool UpdateOrderInWeclapp(WOrder order);
        WOrder GetWeclappOrder(Order order);
        bool UpdateWOrderStatus(WOrder order, int oldStatus);
    }
}