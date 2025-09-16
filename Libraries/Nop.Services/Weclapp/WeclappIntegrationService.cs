using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Services.Weclapp.model;
using System.Linq;
using System.Net;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using Nop.Core.Domain.Customers;
using System.Xml.Linq;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Reflection;
using Nop.Services.Logging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Nop.Core.Domain.Shipping;
using StackExchange.Redis;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using Castle.Core.Resource;
using Remotion.Linq.Clauses;
using Nop.Core.Domain.Logging;
using System.Security.Policy;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StackExchange.Profiling.Internal;

namespace Nop.Services.Weclapp
{
    public class WeclappIntegrationService : IWeclappIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly string weclappApiKey;
        private readonly string weclappArticleApi;
        private readonly string weclappCustomerApi;
        private readonly string weclappOrderApi;
        private readonly string weclappBaseAPI;

        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICategoryService _categoryService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IConfiguration Configuration;
        private readonly IExportLogService _exportLogService;
        private readonly ILogger _logger;
        //private readonly ITierPriceService _tierPriceService;

        //public CustomProductService(IProductService productService, ITierPriceService tierPriceService)
        //{
        //    _productService = productService;
        //    _tierPriceService = tierPriceService;
        //}
        public WeclappIntegrationService(
        IProductService productService,
        ICustomerService customerService,
        IOrderService orderService,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        ICategoryService categoryService,
        IPictureService pictureService,
        IUrlRecordService urlRecordService,
        IStoreMappingService storeMappingService,
        IGenericAttributeService genericAttributeService,
        IExportLogService exportLogService,
        ILogger logger,
        IConfiguration configuration)
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
            _exportLogService = exportLogService;
            _logger = logger;
            Configuration = configuration;
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var InnerHandler = new SocketsHttpHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            _httpClient = new HttpClient(handler);

            weclappApiKey = Configuration["Weclapp:weclappApiKey"];
            _httpClient.DefaultRequestHeaders.Add("AuthenticationToken", weclappApiKey);
            weclappCustomerApi = Configuration["Weclapp:weclappCustomerApi"];
            weclappArticleApi = Configuration["Weclapp:weclappArticleApi"];
            weclappOrderApi = Configuration["Weclapp:weclappOrderApi"];
            weclappBaseAPI = Configuration["Weclapp:weclappBaseAPI"];

            LoadConfigs();
        }

        public void LoadConfigs()
        {
            if (string.IsNullOrEmpty(Configs.CurrencyId))
            {
                Configs.CurrencyId = GetApiId("currency?name-eq=EUR", "id");
                Configs.WarehouseId = GetApiId("warehouse?warehouseType-eq=STANDARD", "id");
                Configs.WarehouseName = GetApiId("warehouse?warehouseType-eq=STANDARD", "name");
                Configs.TaxId = GetApiId("tax?taxKey-eq=DE_ADD_STANDARD", "id");
                Configs.TaxName = GetApiId("tax?taxKey-eq=DE_ADD_STANDARD", "name");
                Configs.NoTaxId = GetApiId("tax?taxKey-eq=DE_ADD_TAX_FREE_EU", "id");
                Configs.NoTaxName = GetApiId("tax?taxKey-eq=DE_ADD_TAX_FREE_EU", "name");
                Configs.UnitId = GetApiId("unit?name-eq=Stk.", "id");
                //Configs.UnitName = GetApiId("/unit?name-eq=pc.", "name");
                Configs.UserId = GetApiId("user", "id");
                Configs.UserName = GetApiId("user", "username");
                Configs.FulfillmentProviderID = GetApiId("fulfillmentProvider", "id");
            }

        }
        public string GetApiId(string area, string field)
        {
            string value = "";
            HttpResponseMessage response = _httpClient.GetAsync(weclappBaseAPI + area).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {   // found
                dynamic jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult().Trim();
                var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                JArray results = (JArray)jsonObject["result"];

                if (results.Count > 0)
                {
                    value = results[0][field].ToString();
                }
            }
            return value;
        }
        #region Products

        public Result SaveProductToWeclapp(IList<Core.Domain.Catalog.Product> products)
        {
            var exportBatch = new ExportBatch() { EntityType = WEntityTypes.PRODUCT, Inserted = 0, Updated = 0, Failed = 0 };
            _exportLogService.InsertExportBatch(exportBatch);
            var result = new Result();
            SaveProductToWeclapp(products, exportBatch);
            _exportLogService.UpdateExportBatch(exportBatch);
            return result;
        }
        public Result SaveProductToWeclapp(IList<Core.Domain.Catalog.Product> products, ExportBatch exportBatch)
        {
            //var exportBatch = new ExportBatch() { EntityType = WEntityTypes.PRODUCT, Inserted = 0, Updated = 0, Failed = 0 };
            //_exportLogService.InsertExportBatch(exportBatch);
            var result = new Result();
            foreach (var product in products)
            {
                result = SaveProductToWeclapp(product, exportBatch.Id);
                if (result.Success && result.Insert)
                    exportBatch.Inserted++;
                else if (result.Success && result.Insert)
                    exportBatch.Updated++;
                else
                    exportBatch.Failed++;
            }
            //_exportLogService.UpdateExportBatch(exportBatch);
            return result;
        }

        public Result SaveProductToWeclapp(Core.Domain.Catalog.Product product, int batchId)
        {
            var result = new Result() { Insert = true, Success = true };
            WProduct wProduct = GetWeclappProduct(product);
            wProduct.batchId = batchId;
            if (string.IsNullOrEmpty(wProduct.id))
            {
                string savedId = CreateProductInWeclapp(wProduct);
                result.Success = !string.IsNullOrEmpty(savedId);
                product.Wid = result.Success ? savedId : "-1";

            }
            else
            {
                result.Insert = true;
                result.Success = UpdateProductInWeclapp(wProduct);
            }
            product.WSynced = null;
            _productService.UpdateProduct(product);
            return result;
        }

        public string CreateProductInWeclapp(WProduct wProduct)
        {
            string savedId = "";
            try
            {
                string json = JsonConvert.SerializeObject(wProduct);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync(weclappArticleApi, content).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wProduct.batchId,
                        EntityType = WEntityTypes.PRODUCT,
                        ActionType = WActionTypes.INSERT,
                        EntityId = wProduct.nopid,
                        Success = true,
                        Details = ""
                    });
                    var responseJson = response.Content.ReadAsStringAsync().Result;
                    dynamic result = JsonConvert.DeserializeObject(responseJson);
                    return result.id;
                }
                else
                {
                    var errorResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wProduct.batchId,
                        EntityType = WEntityTypes.PRODUCT,
                        ActionType = WActionTypes.INSERT,
                        EntityId = wProduct.nopid,
                        Success = false,
                        Details = errorResponse
                    });
                }
            }
            catch (Exception ex)
            {
                var errorResponse = ex.Message;
                _exportLogService.InsertExportBatchLog(new ExportBatchLog
                {
                    ExportBatchId = wProduct.batchId,
                    EntityType = WEntityTypes.PRODUCT,
                    ActionType = WActionTypes.INSERT,
                    EntityId = wProduct.nopid,
                    Success = false,
                    Details = errorResponse
                });
            }

            return savedId;
        }
        public bool UpdateProductInWeclapp(WProduct wProduct)
        {
            try
            {
                string json = JsonConvert.SerializeObject(wProduct);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string updateUrl = $"{weclappArticleApi}/id/{wProduct.id}";

                HttpResponseMessage response = _httpClient.PutAsync(updateUrl, content).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wProduct.batchId,
                        EntityType = WEntityTypes.PRODUCT,
                        ActionType = WActionTypes.UPDATE,
                        EntityId = wProduct.nopid,
                        Success = true,
                        Details = ""
                    });
                    return true;
                }
                else
                {
                    var errorResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wProduct.batchId,
                        EntityType = WEntityTypes.PRODUCT,
                        ActionType = WActionTypes.UPDATE,
                        EntityId = wProduct.nopid,
                        Success = false,
                        Details = errorResponse
                    });
                }
            }
            catch (Exception ex)
            {
                var errorResponse = ex.Message;
                _exportLogService.InsertExportBatchLog(new ExportBatchLog
                {
                    ExportBatchId = wProduct.batchId,
                    EntityType = WEntityTypes.PRODUCT,
                    ActionType = WActionTypes.UPDATE,
                    EntityId = wProduct.nopid,
                    Success = false,
                    Details = errorResponse
                });
            }
            return false;
        }

        public WProduct GetWeclappProduct(Core.Domain.Catalog.Product product)
        {
            WProduct wProduct = new WProduct();
            try
            {
                if (!string.IsNullOrEmpty(product.Wid)) // already created in Weclapp
                {
                    var no = product.Id.ToString("00");
                    string searchUrl = $"{weclappArticleApi}?articleNumber-eq={no}";

                    HttpResponseMessage response = _httpClient.GetAsync(searchUrl).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {   // found
                        dynamic jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult().Trim();
                        var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                        var products = JsonConvert.DeserializeObject<List<WProduct>>(jsonObject.result.ToString());
                        if (products != null && products.Count > 0)
                        {
                            wProduct = products[0];
                        }
                    }
                }
                CopyProduct(product, wProduct);
            }
            catch (Exception ex)
            {
                _logger.Warning($"❌ Error GetWeclappProduct product {product}: {ex.Message}");
            }
            return wProduct;
        }

        public WProduct CopyProduct(Core.Domain.Catalog.Product nopProduct, WProduct wProduct)
        {
            try
            {

                var seoUrl = _urlRecordService.GetSeName(nopProduct, 0, true);

                wProduct.nopid = nopProduct.Id.ToString("00");
                wProduct.name = nopProduct.Name;
                wProduct.articleNumber = nopProduct.Id.ToString("00");
                wProduct.ean = GetArticleEAN(nopProduct);
                //articleImages = source.Images;
                wProduct.availableInSale = true;// nopProduct.Published;
                wProduct.active = true;// nopProduct.Published;
                seoUrl = seoUrl;
                if (!string.IsNullOrEmpty(nopProduct.Wid))
                { // update
                    var wProductPrice = new WProductPrice();
                    if (wProduct.articlePrices != null && wProduct.articlePrices.Count > 0)
                        wProductPrice = wProduct.articlePrices[0];
                    wProductPrice.currencyId = Configs.CurrencyId;
                    wProductPrice.price = nopProduct.Price;
                    wProductPrice.priceType = Configs.PriceType;
                    wProductPrice.priceScaleType = Configs.PriceScaleType;
                    wProductPrice.priceScaleValue = "1";
                    wProductPrice.salesChannel = Configs.SaleChannel;
                }
                else
                {// Doesn't exist in Weclapp, create
                    wProduct.articlePrices = new List<WProductPrice>
                {
                    new WProductPrice {
                        currencyId = Configs.CurrencyId,
                        price = nopProduct.Price,
                        priceType = Configs.PriceType,
                        priceScaleType=Configs.PriceScaleType ,
                        priceScaleValue="1",
                        salesChannel=Configs.SaleChannel }, // ✅ Sale Price
                };
                    wProduct.applyCashDiscount = false;
                    wProduct.articleType = Configs.ArticleType;
                    wProduct.batchNumberRequired = false;
                    wProduct.billOfMaterialPartDeliveryPossible = false;
                    wProduct.defaultPriceCalculationType = Configs.PriceCalculationType;
                    wProduct.defineIndividualTaskTemplates = false;
                    wProduct.productionArticle = false;
                    wProduct.productionConfigurationRule = Configs.ProductionConfigurationRule;
                    wProduct.serialNumberRequired = false;
                    wProduct.showOnDeliveryNote = false;
                    wProduct.taxRateType = Configs.Standard;
                    wProduct.unitName = Configs.UnitName;
                    wProduct.unitId = Configs.UnitId;
                    wProduct.useAvailableForSalesChannels = false;
                    wProduct.useSalesBillOfMaterialItemPrices = false;
                    wProduct.useSalesBillOfMaterialItemPricesForPurchase = false;
                    wProduct.useSalesBillOfMaterialSubitemCosts = false;

                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"❌ Copy product {nopProduct}: {ex.Message}");
            }

            return wProduct;
        }
        #endregion

        private string GetArticleEAN(Core.Domain.Catalog.Product article)
        {
            //return (article.Sku ?? "0") + article.Id.ToString();
            var ean = article.Sku.HasValue() ? article.Sku.Replace(" ", "").Replace("+", "_").Replace("/", "_") : "";
            ean = ean.Length > 13 ? ean.Substring(0, 13) : ean.PadRight(13, '0');
            return ean;
        }

        #region Customers

        public Result SaveCustomerToWeclapp(IList<Customer> customers)
        {
            var exportBatch = new ExportBatch() { EntityType = WEntityTypes.CUSTOMER, Inserted = 0, Updated = 0, Failed = 0 };
            _exportLogService.InsertExportBatch(exportBatch);
            var result = new Result();
            var newCustomers = new List<Customer>();
            foreach (var customer in customers)
            {
                result = SaveCustomerToWeclapp(customer, exportBatch.Id);
                if (result.Success && result.Insert)
                    exportBatch.Inserted++;
                else if (result.Success && result.Insert)
                    exportBatch.Updated++;
                else
                    exportBatch.Failed++;
            }
            _exportLogService.UpdateExportBatch(exportBatch);
            return result;
        }
        public Result SaveCustomerToWeclapp(Customer customer, int batchId)
        {
            var result = new Result() { Insert = true, Success = true };
            WCustomer wCustomer = GetWeclappCustomer(customer);
            if (string.IsNullOrEmpty(wCustomer.id))
            {
                wCustomer.batchId = batchId;
                string savedId = CreateCustomerInWeclapp(wCustomer);
                result.Success = !string.IsNullOrEmpty(savedId);
                customer.Wid = result.Success ? savedId : "-1";

            }
            else
            {
                result.Insert = false;
                result.Success = UpdateCustomerInWeclapp(wCustomer);
            }
            customer.WSynced = null;
            _customerService.UpdateCustomer(customer);
            return result;
        }

        //}
        public string CreateCustomerInWeclapp(WCustomer wCustomer)
        {
            string savedId = "";
            try
            {
                //var json = MapNopCommerceCustomerToWeclappArticle(customer);
                string json = JsonConvert.SerializeObject(wCustomer);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync(weclappCustomerApi, content).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wCustomer.batchId,
                        EntityType = WEntityTypes.CUSTOMER,
                        ActionType = WActionTypes.INSERT,
                        EntityId = wCustomer.nopid,
                        Success = true,
                        Details = ""
                    });
                    var responseJson = response.Content.ReadAsStringAsync().Result;
                    dynamic result = JsonConvert.DeserializeObject(responseJson);
                    return result.id;
                }
                else
                {
                    var errorResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wCustomer.batchId,
                        EntityType = WEntityTypes.CUSTOMER,
                        ActionType = WActionTypes.INSERT,
                        EntityId = wCustomer.nopid,
                        Success = false,
                        Details = errorResponse
                    });
                }
            }
            catch (Exception ex)
            {
                var errorResponse = ex.Message;
                _exportLogService.InsertExportBatchLog(new ExportBatchLog
                {
                    ExportBatchId = wCustomer.batchId,
                    EntityType = WEntityTypes.CUSTOMER,
                    ActionType = WActionTypes.INSERT,
                    EntityId = wCustomer.nopid,
                    Success = false,
                    Details = errorResponse
                });
            }

            return savedId;
        }
        public bool UpdateCustomerInWeclapp(WCustomer wCustomer)
        {
            try
            {
                string json = JsonConvert.SerializeObject(wCustomer);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string updateUrl = $"{weclappCustomerApi}/id/{wCustomer.id}";

                HttpResponseMessage response = _httpClient.PutAsync(updateUrl, content).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wCustomer.batchId,
                        EntityType = WEntityTypes.CUSTOMER,
                        ActionType = WActionTypes.UPDATE,
                        EntityId = wCustomer.nopid,
                        Success = true,
                        Details = ""
                    });
                    return true;
                }
                else
                {
                    var errorResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wCustomer.batchId,
                        EntityType = WEntityTypes.CUSTOMER,
                        ActionType = WActionTypes.UPDATE,
                        EntityId = wCustomer.nopid,
                        Success = false,
                        Details = errorResponse
                    });
                }
            }
            catch (Exception ex)
            {
                var errorResponse = ex.Message;
                _exportLogService.InsertExportBatchLog(new ExportBatchLog
                {
                    ExportBatchId = wCustomer.batchId,
                    EntityType = WEntityTypes.CUSTOMER,
                    ActionType = WActionTypes.UPDATE,
                    EntityId = wCustomer.nopid,
                    Success = false,
                    Details = errorResponse
                });
            }

            return false;
        }

        public WCustomer GetWeclappCustomer(Customer customer)
        {
            WCustomer wCustomer = new WCustomer();
            try
            {
                if (!string.IsNullOrEmpty(customer.Wid)) // already created in Weclapp
                {
                    var no = customer.Id.ToString();
                    string searchUrl = $"{weclappCustomerApi}?customerNumber-eq={no}";

                    HttpResponseMessage response = _httpClient.GetAsync(searchUrl).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {   // found
                        dynamic jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult().Trim();
                        var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                        var customers = JsonConvert.DeserializeObject<List<WCustomer>>(jsonObject.result.ToString());
                        if (customers != null && customers.Count > 0)
                        {
                            wCustomer = customers[0];
                        }
                    }
                }
                CopyCustomer(customer, wCustomer);
            }
            catch (Exception ex)
            {
                _logger.Warning($"❌ Error GetWeclappCustomer customer {customer.Id}: {ex.Message}");
            }
            return wCustomer;
        }

        public WCustomer CopyCustomer(Customer source, WCustomer target)
        {
            Address address = null;
            if (source.Addresses.Count > 0)
                address = source.Addresses[0];
            else if (source.ShippingAddress != null)
                address = source.ShippingAddress;
            if (address != null)
            {
                var vatNumber = _genericAttributeService.GetAttribute<string>(source, NopCustomerDefaults.VatNumberAttribute);
                target.customerNumber = source.Id.ToString();
                target.nopid = source.Id.ToString();
                target.email = source.Email;
                target.vatRegistrationNumber = vatNumber;
                target.company = address == null ? "" : address.Company;
                target.name = address == null ? "" : address.Company;
                if (!string.IsNullOrEmpty(source.Wid))
                { // update
                    var wAddress = new WAddress();
                    if (target.addresses.Count > 0)
                        wAddress = target.addresses[0];
                    wAddress.street1 = source.BillingAddress?.Address1 ?? "N/A";
                    wAddress.zipCode = source.BillingAddress?.ZipPostalCode ?? "00000";
                    wAddress.city = source.BillingAddress?.City ?? "N/A";
                    wAddress.country = source.BillingAddress?.Country.Name;
                    wAddress.countryCode = source.BillingAddress?.Country?.TwoLetterIsoCode ?? "DE";
                    wAddress.primeAddress = true;
                }
                else
                {// Doesn't exist in Weclapp, create
                    target.addresses = new List<WAddress>
                       {
                        new WAddress
                        {
                            street1 = source.BillingAddress?.Address1 ?? "N/A",
                            zipCode = source.BillingAddress?.ZipPostalCode ?? "00000",
                            city = source.BillingAddress?.City ?? "N/A",
                            country = source.BillingAddress?.Country.Name,
                            countryCode = source.BillingAddress?.Country?.TwoLetterIsoCode ?? "DE",
                            primeAddress=true
                        }
                    };
                    target.partyType = Configs.PartyType;
                    target.customerType = Configs.CustomerType;
                    target.blocked = false;
                    target.deliveryBlock = false;
                    target.insolvent = false;
                    target.insured = false;
                    target.invoiceBlock = false;
                    target.optIn = false;
                    target.optInLetter = false;
                    target.optInPhone = false;
                    target.optInSms = false;
                    target.responsibleUserFixed = false;
                    target.useCustomsTariffNumber = false;
                }
            }
            return target;
        }
        #endregion

        #region Orders

        public Result SaveOrderToWeclapp(IList<Core.Domain.Orders.Order> orders)
        {
            var exportBatch = new ExportBatch() { EntityType = WEntityTypes.ORDER, Inserted = 0, Updated = 0, Failed = 0 };
            _exportLogService.InsertExportBatch(exportBatch);
            var result = new Result() { Insert = true, Success = true };
            var newOrders = new List<Core.Domain.Orders.Order>();
            foreach (var order in orders)
            {
                var customer = order.Customer;
                if (string.IsNullOrEmpty(customer.Wid))
                    SaveCustomerToWeclapp(customer, exportBatch.Id);
                var products = order.OrderItems.Where(a => string.IsNullOrEmpty(a.Product.Wid)).Select(i => i.Product).ToList();
                if (products.Count > 0)
                    SaveProductToWeclapp(products, exportBatch);
                WOrder wOrder = GetWeclappOrder(order);
                wOrder.batchId = exportBatch.Id;
                if (string.IsNullOrEmpty(wOrder.id))
                {
                    string savedId = CreateOrderInWeclapp(wOrder);
                    result.Success = !string.IsNullOrEmpty(savedId);
                    if (result.Success)
                    {
                        wOrder.id = savedId;
                        order.Wid = savedId;
                        newOrders.Add(order);
                        UpdateWOrderStatus(wOrder, order.OrderStatusId);
                    }
                    else
                    {
                        order.Wid = "-1";
                        newOrders.Add(order);
                    }
                }
                else
                {
                    result.Insert = false;
                    result.Success = UpdateOrderInWeclapp(wOrder);
                }
                order.WSynced = null;
                newOrders.Add(order);
                if (result.Success && result.Insert)
                    exportBatch.Inserted++;
                else if (result.Success && !result.Insert)
                    exportBatch.Updated++;
                else
                    exportBatch.Failed++;
            }
            _orderService.UpdateOrders(newOrders);
            _exportLogService.UpdateExportBatch(exportBatch);
            return result;
        }

        //}
        public string CreateOrderInWeclapp(WOrder wOrder)
        {
            string savedId = "";
            try
            {
                var wo = new NewOrder();
                wOrder.CopyProperties(wo);
                wo.orderItems = new List<NewOrderItem>();
                if (wOrder.orderItems != null)
                {
                    foreach (var oitem in wOrder.orderItems)
                    {
                        var woi = new NewOrderItem();
                        oitem.CopyProperties(woi);
                        wo.orderItems.Add(woi);
                    }
                    if (wOrder.shippingCostItems != null && wOrder.shippingCostItems.Count > 0)
                    {
                        wo.shippingCostItems = new List<NewOrderItem>();
                        var wosc = new NewOrderItem();
                        wOrder.shippingCostItems[0].CopyProperties(wosc);
                        wo.shippingCostItems.Add(wosc);
                    }
                }

                string json = JsonConvert.SerializeObject(wo);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync(weclappOrderApi, content).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wOrder.batchId,
                        EntityType = WEntityTypes.ORDER,
                        ActionType = WActionTypes.INSERT,
                        EntityId = wOrder.nopid,
                        Success = true,
                        Details = ""
                    });
                    var responseJson = response.Content.ReadAsStringAsync().Result;
                    dynamic result = JsonConvert.DeserializeObject(responseJson);
                    return result.id;
                }
                else
                {
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wOrder.batchId,
                        EntityType = WEntityTypes.ORDER,
                        ActionType = WActionTypes.INSERT,
                        EntityId = wOrder.nopid,
                        Success = false,
                        Details = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                    });
                }
            }
            catch (Exception ex)
            {
                _exportLogService.InsertExportBatchLog(new ExportBatchLog
                {
                    ExportBatchId = wOrder.batchId,
                    EntityType = WEntityTypes.ORDER,
                    ActionType = WActionTypes.INSERT,
                    EntityId = wOrder.nopid,
                    Success = false,
                    Details = ex.Message
                });
            }

            return savedId;
        }

        public bool UpdateWOrderStatus(WOrder wOrder, int oldstatus)
        {
            var updated = false;
            var method = "";
            var wOrderStatus = WOrderStatus.IN_PROGRESS;
            if (oldstatus == (int)OrderStatus.Cancelled)
            {
                wOrderStatus = WOrderStatus.CANCELLED;
                method = "cancelOrManuallyClose";
            }
            else if (oldstatus == (int)OrderStatus.Complete)
            {
                wOrderStatus = WOrderStatus.MANUALLY_CLOSED;
                method = "manuallyClose";
            }
            if (wOrderStatus != WOrderStatus.IN_PROGRESS)
            {
                try
                {
                    var updateData = new
                    {
                        id = wOrder.id
                    };

                    string json = JsonConvert.SerializeObject(updateData);
                    var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                    string updateUrl = $"{weclappOrderApi}/id/{wOrder.id}/{method}";
                    var response = _httpClient.PostAsync(updateUrl, jsonContent).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        _exportLogService.InsertExportBatchLog(new ExportBatchLog
                        {
                            ExportBatchId = wOrder.batchId,
                            EntityType = WEntityTypes.ORDER,
                            ActionType = WActionTypes.UPDATE,
                            EntityId = wOrder.nopid,
                            Success = true,
                            Details = ""
                        });
                        updated = true;
                    }
                    else
                    {
                        var errorResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        _exportLogService.InsertExportBatchLog(new ExportBatchLog
                        {
                            ExportBatchId = wOrder.batchId,
                            EntityType = WEntityTypes.ORDER,
                            ActionType = WActionTypes.UPDATE,
                            EntityId = wOrder.nopid,
                            Success = false,
                            Details = errorResponse
                        });
                    }
                }
                catch (Exception ex)
                {
                    var errorResponse = ex.Message;
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wOrder.batchId,
                        EntityType = WEntityTypes.ORDER,
                        ActionType = WActionTypes.UPDATE,
                        EntityId = wOrder.nopid,
                        Success = false,
                        Details = errorResponse
                    });
                }
            }
            return updated;
        }
        public bool UpdateOrderInWeclapp(WOrder wOrder)
        {
            try
            {
                string json = JsonConvert.SerializeObject(wOrder);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string updateUrl = $"{weclappOrderApi}/id/{wOrder.id}";

                HttpResponseMessage response = _httpClient.PutAsync(updateUrl, content).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wOrder.batchId,
                        EntityType = WEntityTypes.ORDER,
                        ActionType = WActionTypes.UPDATE,
                        EntityId = wOrder.nopid,
                        Success = true,
                        Details = ""
                    });
                    return true;
                }
                else
                {
                    var errorResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _exportLogService.InsertExportBatchLog(new ExportBatchLog
                    {
                        ExportBatchId = wOrder.batchId,
                        EntityType = WEntityTypes.ORDER,
                        ActionType = WActionTypes.UPDATE,
                        EntityId = wOrder.nopid,
                        Success = false,
                        Details = errorResponse
                    });
                }
            }
            catch (Exception ex)
            {
                var errorResponse = ex.Message;
                _exportLogService.InsertExportBatchLog(new ExportBatchLog
                {
                    ExportBatchId = wOrder.batchId,
                    EntityType = WEntityTypes.ORDER,
                    ActionType = WActionTypes.UPDATE,
                    EntityId = wOrder.nopid,
                    Success = false,
                    Details = errorResponse
                });
            }
            return false;
        }

        public WOrder GetWeclappOrder(Core.Domain.Orders.Order order)
        {
            var wOrder = new WOrder();
            try
            {
                if (!string.IsNullOrEmpty(order.Wid)) // already created in Weclapp
                {
                    var no = order.Id.ToString("000");
                    string searchUrl = $"{weclappOrderApi}?orderNumber-eq={no}";

                    HttpResponseMessage response = _httpClient.GetAsync(searchUrl).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {   // found
                        dynamic jsonResponse = response.Content.ReadAsStringAsync().GetAwaiter().GetResult().Trim();
                        var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                        wOrder = new WOrder();
                        var orders = JsonConvert.DeserializeObject<List<WOrder>>(jsonObject.result.ToString());
                        if (orders != null && orders.Count > 0)
                        {
                            wOrder = orders[0];
                        }
                    }
                }
                CopyOrder(order, wOrder);
            }
            catch (Exception ex)
            {
                _logger.Warning($"❌ Error GetWeclappOrder order {order.Id}: {ex.Message}");
            }
            return wOrder;
        }

        public WOrder CopyOrder(Core.Domain.Orders.Order nopOrder, WOrder wOrder)
        {
            if (string.IsNullOrEmpty(nopOrder.Wid))
                wOrder = (WOrder)wOrder;
            if (nopOrder.ShippingAddress != null && !string.IsNullOrEmpty(nopOrder.ShippingAddress.Company))
            {
                //target.customerOrderNumber = source.OrderGuid.ToString();
                wOrder.nopid = nopOrder.Id.ToString();
                wOrder.customerNumber = nopOrder.Customer.Id.ToString();
                wOrder.customerId = nopOrder.Customer?.Wid.ToString();
                wOrder.creatorId = Configs.UserId;
                //target.currency = source.CustomerCurrencyCode;
                wOrder.netAmount = nopOrder.OrderTotal;
                wOrder.netAmountInCompanyCurrency = nopOrder.OrderTotal;
                wOrder.grossAmount = nopOrder.OrderTotal;
                wOrder.grossAmountInCompanyCurrency = nopOrder.OrderTotal;

                wOrder.orderNumber = nopOrder.Id.ToString("000");
                //wOrder.status = WOrderStatus.IN_PROGRESS;
                //if (nopOrder.OrderStatusId == (int)OrderStatus.Cancelled)
                //    wOrder.status = WOrderStatus.CANCELLED;
                //else if (nopOrder.OrderStatusId == (int)OrderStatus.Complete)
                //    wOrder.status = WOrderStatus.MANUALLY_CLOSED;
                if (wOrder.orderItems == null)
                    wOrder.orderItems = new List<WOrderItem>();
                //if (!string.IsNullOrEmpty(source.Wid))
                { // update
                    foreach (var oItem in nopOrder.OrderItems)
                    {
                        var wItem = wOrder.orderItems.SingleOrDefault(i => i.articleNumber.ToString() == (oItem.Product.Id.ToString()));
                        if (wItem != null)
                        {
                            wItem.articleNumber = oItem.Product.Id.ToString("00");
                            wItem.title = oItem.Product.Name;
                            wItem.articleId = oItem.Product.Wid ?? oItem.Product.Id.ToString();
                            wItem.quantity = oItem.Quantity.ToString();
                            wItem.unitPrice = oItem.UnitPriceExclTax;
                            wItem.unitPriceInCompanyCurrency = oItem.UnitPriceExclTax;
                            wItem.netAmountForStatisticsInCompanyCurrency = oItem.PriceExclTax;
                            wItem.netAmountInCompanyCurrency = oItem.PriceExclTax;
                            wItem.netAmountForStatistics = oItem.PriceExclTax;
                            wItem.netAmount = oItem.PriceExclTax;
                            wItem.grossAmount = oItem.PriceExclTax;
                            wItem.grossAmountInCompanyCurrency = oItem.PriceExclTax;
                            wItem.manualUnitPrice = true;
                            wItem.taxName = oItem.PriceInclTax == oItem.PriceExclTax ? Configs.NoTaxName : Configs.TaxName;
                            wItem.taxId = oItem.PriceInclTax == oItem.PriceExclTax ? Configs.NoTaxId : Configs.TaxId;
                            //wItem.lastModifiedDate = ((Int64)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString(new CultureInfo("en-US"));
                        }
                        else
                        {
                            wOrder.orderItems.Add(
                            new WOrderItem
                            {
                                //title = oItem.Product.Name,
                                articleNumber = oItem.Product.Id.ToString("00"),
                                articleId = oItem.Product.Wid == null ? "" : oItem.Product?.Wid.ToString(),
                                quantity = oItem.Quantity.ToString(),
                                unitPrice = oItem.UnitPriceExclTax,
                                unitPriceInCompanyCurrency = oItem.UnitPriceExclTax,
                                netAmountForStatisticsInCompanyCurrency = oItem.PriceExclTax,
                                netAmountInCompanyCurrency = oItem.PriceExclTax,
                                netAmountForStatistics = oItem.PriceExclTax,
                                netAmount = oItem.PriceExclTax,
                                grossAmount = oItem.PriceExclTax,
                                grossAmountInCompanyCurrency = oItem.PriceExclTax,
                                manualUnitPrice = true,
                                lastModifiedDate = TimeSecs(),
                                createdDate = TimeSecs(nopOrder.CreatedOnUtc),
                                unitId = Configs.UnitId,
                                unitName = Configs.UnitName,
                                warehouseId = Configs.WarehouseId,
                                warehouseName = Configs.WarehouseName,
                                taxId = oItem.PriceInclTax == oItem.PriceExclTax ? Configs.NoTaxId : Configs.TaxId,
                                taxName = oItem.PriceInclTax == oItem.PriceExclTax ? Configs.NoTaxName : Configs.TaxName,
                            });
                        }
                    }
                }
                if (string.IsNullOrEmpty(nopOrder.Wid))
                {//new
                    wOrder.salesOrderPaymentType = Configs.Standard;
                    wOrder.template = false;
                    wOrder.applyShippingCostsOnlyOnce = false;
                    wOrder.recordOpeningInheritance = false;
                    wOrder.recordFreeTextInheritance = false;
                    wOrder.recordCommentInheritance = false;
                    wOrder.factoring = false;
                    wOrder.sentToRecipient = false;
                    wOrder.disableEmailTemplate = false;
                    wOrder.currencyConversionDate = TimeSecs(nopOrder.CreatedOnUtc);
                    wOrder.plannedShippingDate = TimeSecs(nopOrder.CreatedOnUtc);
                    wOrder.pricingDate = TimeSecs(nopOrder.CreatedOnUtc);
                    wOrder.orderDate = TimeSecs(nopOrder.CreatedOnUtc);
                    wOrder.orderDate = wOrder.orderDate + "000";
                    wOrder.createdDate = TimeSecs(nopOrder.CreatedOnUtc);
                    wOrder.createdDate = wOrder.createdDate + "000";
                    wOrder.recordCurrencyId = Configs.CurrencyId;
                    wOrder.responsibleUserId = Configs.UserId;
                    wOrder.responsibleUserUsername = Configs.UserName;
                    wOrder.warehouseId = Configs.WarehouseId;
                    wOrder.warehouseName = Configs.WarehouseName;
                    wOrder.fulfillmentProviderId = Configs.FulfillmentProviderID;
                }
                else
                {

                }

                if (nopOrder.OrderShippingInclTax > 0 && wOrder.shippingCostItems.Count==0)
                {
                    if (wOrder.shippingCostItems == null)
                        wOrder.shippingCostItems = new List<WOrderItem>();

                    wOrder.shippingCostItems.Add(
                    new WOrderItem
                    {
                        unitPrice = nopOrder.OrderShippingInclTax,
                        unitPriceInCompanyCurrency = nopOrder.OrderShippingInclTax,
                        netAmountForStatisticsInCompanyCurrency = nopOrder.OrderShippingInclTax,
                        netAmountInCompanyCurrency = nopOrder.OrderShippingInclTax,
                        netAmountForStatistics = nopOrder.OrderShippingInclTax,
                        netAmount = nopOrder.OrderShippingInclTax,
                        grossAmount = nopOrder.OrderShippingInclTax,
                        grossAmountInCompanyCurrency = nopOrder.OrderShippingInclTax,
                        manualUnitPrice = true,
                        lastModifiedDate = TimeSecs(),
                        createdDate = TimeSecs(nopOrder.CreatedOnUtc),
                        taxId = Configs.NoTaxId,
                        taxName = Configs.NoTaxName,
                    });

                }
            }
            return wOrder;
        }
        private string TimeSecs(DateTime? dt = null)
        {
            if (dt == null) dt = DateTime.Now;
            return ((Int64)dt.Value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString(new CultureInfo("en-US"));
        }
        #endregion


    }
    public class Result
    {
        public bool Success { get; set; }
        public bool Insert { get; set; }
    }
}