using System;
using System.Threading.Tasks;
using Nop.Services.Caching;
using Nop.Services.Tasks;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Weclapp.model;
using System.Collections.Generic;
using Nop.Services.Logging;

namespace Nop.Services.Weclapp
{
    public class WeclappSyncTask : IScheduleTask, IWeclappSyncTask
    {
        private readonly IWeclappIntegrationService _weclappService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICategoryService _categoryService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IExportLogService _exportLogService;

        public WeclappSyncTask(
            IProductService productService,
            ICustomerService customerService,
            IOrderService orderService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            ICategoryService categoryService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IGenericAttributeService genericAttributeService,
            IWeclappIntegrationService weclappService,
            IExportLogService exportLogService)
        {
            _productService = productService;
            _customerService = customerService;
            _orderService = orderService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _categoryService = categoryService;
            _pictureService = pictureService;
            _urlRecordService = urlRecordService;
            _genericAttributeService = genericAttributeService;
            _weclappService = weclappService;
            _exportLogService = exportLogService;
        }
        public void Execute()
        {
            ExecuteProduct();
            ExecuteCustomer();
            ExecuteOrder();
        }
        public bool ExecuteProduct(bool retryFailed = false)
        {
            try
            {
                var lastBatch = _exportLogService.GetLastBatch(WEntityTypes.PRODUCT);
                var updatedAfter = lastBatch != null ? lastBatch.ExportedOn : DateTime.Now.AddDays(-1);
                Console.WriteLine("🔄 Syncing Products with Weclapp...");
                var found = 0;
                do
                {
                    IList<Core.Domain.Catalog.Product> products = new List<Core.Domain.Catalog.Product>();
                    products = _productService.GetWeclappNewProducts(100, updatedAfter, retryFailed);
                    retryFailed = false;
                    found = products.Count;
                    //var products = _productService.GetProductsByIds(new int[] { 5511, 5510, 5509, 5508, 5507, 5506,5343,4735 });
                    //var products = _dataService.GetNopProducts();
                    if (products.Count > 0)
                    {
                        _weclappService.SaveProductToWeclapp(products);
                        Console.WriteLine("✅ Sync completed!");
                    }
                    else
                    {
                        Console.WriteLine("⚠ No products found.");
                        return false;
                    }
                } while (found > 0);

            }
            catch (Exception ex)
            {
            }

            return true;
        }

        public bool ExecuteCustomer(bool retryFailed = false)
        {
            try
            {
                var lastBatch = _exportLogService.GetLastBatch(WEntityTypes.CUSTOMER);
                var updatedAfter = lastBatch != null ? lastBatch.ExportedOn : DateTime.Now.AddDays(-1);
                var found = 0;
                do
                {
                    var customers = _customerService.GetWeclappNewCustomers(100, updatedAfter, retryFailed);
                    found = customers.Count;
                    retryFailed = false;
                    //(new int[] { 21286953, 21229763, 21227714, 21210569, 21180522 });
                    //21286953, 21229763, 21227714, 21210569, 21180522, 21157465, 21136588, 21126354, 21047703, 21021435, 21004579, 20845758, 20751568, 20490355, 20456725
                    Console.WriteLine("🔄 Syncing Customers with Weclapp...");

                    //var customers = _dataService.GetNopCustomers();
                    if (customers.Count > 0)
                    {
                        _weclappService.SaveCustomerToWeclapp(customers);
                        Console.WriteLine("✅ Sync completed!");
                    }
                    else
                    {
                        Console.WriteLine("⚠ No Customers found.");
                        return false;
                    }
                } while (found > 0);
            }
            catch (Exception ex)
            {
            }

            return true;
        }
        public bool ExecuteOrder(bool retryFailed = false)
        {
            try
            {
                var lastBatch = _exportLogService.GetLastBatch(WEntityTypes.ORDER);
                var updatedAfter = lastBatch != null ? lastBatch.ExportedOn : DateTime.Now.AddDays(-1);
                var found = 0;
                do
                {
                    var orders = _orderService.GetWeclappNewOrders(100, updatedAfter, retryFailed);
                    found = orders.Count;
                    retryFailed = false;
                    //.GetOrdersByIds(new int[] { 378440, 378446, 378441, 378443, 378444, 378445 });
                    //378441, 378440, 378439, 378438, 378437
                    //378446,378445,378444,378443,378442,378441,378440,378439,378438,378437
                    Console.WriteLine("🔄 Syncing Orders with Weclapp...");

                    //var orders = _dataService.GetNopOrders();
                    if (orders.Count > 0)
                    {
                        _weclappService.SaveOrderToWeclapp(orders);
                        Console.WriteLine("✅ Sync completed!");
                    }
                    else
                    {
                        Console.WriteLine("⚠ No Orders found.");
                        return false;
                    }
                } while (found > 0);
            }
            catch (Exception ex)
            {
            }
            return true;
        }
    }
}