using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Http.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Areas.Admin.Models.Reports;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using StackExchange.Profiling.Internal;
using Newtonsoft.Json;
using Nop.Services.Customers;
using Nop.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Tax;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class OrderController : BaseAdminController
    {
        #region Fields

        private List<QuickOrderItemModel> SelectedQuickOrderItems = new List<QuickOrderItemModel>();
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressService _addressService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDownloadService _downloadService;
        private readonly IEncryptionService _encryptionService;
        private readonly IExportManager _exportManager;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPaymentService _paymentService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IPdfService _pdfService;
        private readonly IPermissionService _permissionService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly OrderSettings _orderSettings;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IOrderReportService _orderReportService;
        private readonly ITaxService _taxService;
        private List<OrderItemModel> SelectedOrderItems = new List<OrderItemModel>();

        #endregion

        #region Ctor

        public OrderController(IAddressAttributeParser addressAttributeParser,
            IAddressService addressService,
            ICustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            IEncryptionService encryptionService,
            IExportManager exportManager,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IOrderModelFactory orderModelFactory,
            ICustomerModelFactory customerModelFactory,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPriceFormatter priceFormatter,
            IPaymentService paymentService,
            IHttpContextAccessor httpContext,
            IPdfService pdfService,
            IPermissionService permissionService,
            IPriceCalculationService priceCalculationService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            ICustomerService customerService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            OrderSettings orderSettings,
            IProductModelFactory productModelFactory,
            IOrderReportService orderReportService,
            ITaxService taxService)
        {
            _addressAttributeParser = addressAttributeParser;
            _addressService = addressService;
            _customerActivityService = customerActivityService;
            _dateTimeHelper = dateTimeHelper;
            _downloadService = downloadService;
            _encryptionService = encryptionService;
            _exportManager = exportManager;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _orderModelFactory = orderModelFactory;
            _customerModelFactory = customerModelFactory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _paymentService = paymentService;
            _httpContext = httpContext;
            _pdfService = pdfService;
            _permissionService = permissionService;
            _priceCalculationService = priceCalculationService;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _orderSettings = orderSettings;
            _productModelFactory = productModelFactory;
            _orderReportService = orderReportService;
            _taxService = taxService;
        }

        #endregion

        #region Utilities

        protected virtual bool HasAccessToOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (_workContext.CurrentVendor == null)
                //not a vendor; has access
                return true;

            var vendorId = _workContext.CurrentVendor.Id;
            var hasVendorProducts = order.OrderItems.Any(orderItem => orderItem.Product.VendorId == vendorId);
            return hasVendorProducts;
        }

        protected virtual bool HasAccessToOrderItem(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            if (_workContext.CurrentVendor == null)
                //not a vendor; has access
                return true;

            var vendorId = _workContext.CurrentVendor.Id;
            return orderItem.Product.VendorId == vendorId;
        }

        protected virtual bool HasAccessToShipment(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            if (_workContext.CurrentVendor == null)
                //not a vendor; has access
                return true;

            var hasVendorProducts = false;
            var vendorId = _workContext.CurrentVendor.Id;
            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                if (orderItem == null || orderItem.Product.VendorId != vendorId)
                    continue;

                hasVendorProducts = true;
                break;
            }

            return hasVendorProducts;
        }

        /// <summary>
        /// Parse product attributes on the add product to order details page
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form</param>
        /// <param name="errors">Errors</param>
        /// <returns>Parsed attributes</returns>
        protected virtual string ParseProductAttributes(Product product, IFormCollection form, List<string> errors)
        {
            var attributesXml = string.Empty;

            var productAttributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
            foreach (var attribute in productAttributes)
            {
                var controlId = $"{NopAttributePrefixDefaults.Product}{attribute.Id}";
                StringValues ctrlAttributes;

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                            {
                                //get quantity entered by customer
                                var quantity = 1;
                                var quantityStr = form[$"{NopAttributePrefixDefaults.Product}{attribute.Id}_{selectedAttributeId}_qty"];
                                if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                    (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                    errors.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));

                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                            }
                        }

                        break;
                    case AttributeControlType.Checkboxes:
                        ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            foreach (var item in ctrlAttributes.ToString()
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId <= 0)
                                    continue;

                                //get quantity entered by customer
                                var quantity = 1;
                                var quantityStr = form[$"{NopAttributePrefixDefaults.Product}{attribute.Id}_{item}_qty"];
                                if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                    (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                    errors.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));

                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                            }
                        }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        //load read-only (already server-side selected) values
                        var attributeValues = _productAttributeService.GetProductAttributeValues(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                        {
                            //get quantity entered by customer
                            var quantity = 1;
                            var quantityStr = form[$"{NopAttributePrefixDefaults.Product}{attribute.Id}_{selectedAttributeId}_qty"];
                            if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                errors.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));

                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                        }

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, enteredText);
                        }

                        break;
                    case AttributeControlType.Datepicker:

                        var day = form[controlId + "_day"];
                        var month = form[controlId + "_month"];
                        var year = form[controlId + "_year"];
                        DateTime? selectedDate = null;
                        try
                        {
                            selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                        }
                        catch
                        {
                        }

                        if (selectedDate.HasValue)
                        {
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, selectedDate.Value.ToString("D"));
                        }

                        break;
                    case AttributeControlType.FileUpload:

                        Guid.TryParse(form[controlId], out var downloadGuid);
                        var download = _downloadService.GetDownloadByGuid(downloadGuid);
                        if (download != null)
                        {
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, download.DownloadGuid.ToString());
                        }

                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in productAttributes)
            {
                var conditionMet = _productAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }

            return attributesXml;
        }

        /// <summary>
        /// Parse rental dates on the add product to order details page
        /// </summary>
        /// <param name="form">Form</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        protected virtual void ParseRentalDates(IFormCollection form, out DateTime? startDate, out DateTime? endDate)
        {
            startDate = null;
            endDate = null;

            var ctrlStartDate = form["rental_start_date"];
            var ctrlEndDate = form["rental_end_date"];
            try
            {
                const string datePickerFormat = "MM/dd/yyyy";
                startDate = DateTime.ParseExact(ctrlStartDate, datePickerFormat, CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(ctrlEndDate, datePickerFormat, CultureInfo.InvariantCulture);
            }
            catch
            {
            }
        }

        protected virtual void LogEditOrder(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);

            _customerActivityService.InsertActivity("EditOrder",
                string.Format(_localizationService.GetResource("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
        }

        protected virtual string AddGiftCards(IFormCollection form, Product product, string attributesXml,
           out string recipientName, out string recipientEmail, out string senderName, out string senderEmail,
           out string giftCardMessage)
        {
            recipientName = string.Empty;
            recipientEmail = string.Empty;
            senderName = string.Empty;
            senderEmail = string.Empty;
            giftCardMessage = string.Empty;

            if (!product.IsGiftCard)
                return attributesXml;

            foreach (var formKey in form.Keys)
            {
                if (formKey.Equals("giftcard.RecipientName", StringComparison.InvariantCultureIgnoreCase))
                {
                    recipientName = form[formKey];
                    continue;
                }

                if (formKey.Equals("giftcard.RecipientEmail", StringComparison.InvariantCultureIgnoreCase))
                {
                    recipientEmail = form[formKey];
                    continue;
                }

                if (formKey.Equals("giftcard.SenderName", StringComparison.InvariantCultureIgnoreCase))
                {
                    senderName = form[formKey];
                    continue;
                }

                if (formKey.Equals("giftcard.SenderEmail", StringComparison.InvariantCultureIgnoreCase))
                {
                    senderEmail = form[formKey];
                    continue;
                }

                if (formKey.Equals("giftcard.Message", StringComparison.InvariantCultureIgnoreCase))
                {
                    giftCardMessage = form[formKey];
                }
            }

            attributesXml = _productAttributeParser.AddGiftCardAttribute(attributesXml,
                recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);

            return attributesXml;
        }

        #endregion

        #region Order list

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List(List<int> orderStatuses = null, List<int> paymentStatuses = null, List<int> shippingStatuses = null)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //prepare model
            var model = _orderModelFactory.PrepareOrderSearchModel(new OrderSearchModel
            {
                OrderStatusIds = orderStatuses,
                PaymentStatusIds = paymentStatuses,
                ShippingStatusIds = shippingStatuses
            });

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult OrderList(OrderSearchModel searchModel)
        {
            if (_workContext.CurrentCustomer.CustomerRoles.Any(r => r.SystemName == Core.Domain.Customers.NopCustomerDefaults.SalemanRoleName))
                searchModel.SalemanId = _workContext.CurrentCustomer.Id;
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _orderModelFactory.PrepareOrderListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult ReportAggregates(OrderSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _orderModelFactory.PrepareOrderAggregatorModel(searchModel);

            return Json(model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-order-by-number")]
        public virtual IActionResult GoToOrderId(OrderSearchModel model)
        {
            var order = _orderService.GetOrderByCustomOrderNumber(model.GoDirectlyToCustomOrderNumber);

            if (order == null)
                return List();

            return RedirectToAction("Edit", "Order", new { id = order.Id });
        }
        public virtual IActionResult GetOrderItemWithLastPrice(int customerId, int Id, int cart)
        {
            if (cart == 1)
            {
                return GetCartItemWithLastPrice(customerId);
            }
            //a vendor should have access only to his products
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }
            String json = _httpContext.HttpContext.Session.GetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid);
            if (!string.IsNullOrEmpty(json))
            {
                SelectedQuickOrderItems = JsonConvert.DeserializeObject<List<QuickOrderItemModel>>(json);
            }
            if (string.IsNullOrEmpty(json) && Id > 0)
            {
                var order = _orderService.GetOrderById(Id);
                var productIds = order.OrderItems.Select(x => x.ProductId).ToArray();
                var lastPrices = _orderService.GetLastPricesByCustomer(customerId, productIds);

                // Get Existing Order item List
                var orderItems = (from oItem in order.OrderItems
                                  select new
                                  {
                                      ProductId = oItem.ProductId,
                                      ProductName = oItem.Product.Name,
                                      NewStockComing = oItem.Product.NewStockComing,
                                      Sku = oItem.Product.Sku,
                                      MinPrice = oItem.Product.MinPrice,
                                      OldPrice = oItem.Product.OldPrice,
                                      //  OldPriceIncTax = oItem.OldPrice,
                                      UnitPriceExclTax = oItem.UnitPriceExclTax,
                                      UnitPriceInclTax = _taxService.GetProductPrice(oItem.Product, oItem.UnitPriceExclTax, true, order.Customer, out _),
                                      DiscountAmountExclTax = oItem.DiscountAmountExclTax,
                                      Quantity = oItem.Quantity,
                                      StockQuantity = oItem.Product.StockQuantity,
                                      PurchasePrice = oItem.Product.ProductCost

                                  })
               .OrderByDescending(x => x.ProductName)
               .Select(item => new QuickOrderItemModel
               {
                   ProductId = item.ProductId,
                   ProductName = item.ProductName,
                   NewStockComing = item.NewStockComing,
                   Sku = item.Sku,
                   UnitPriceExclTax = item.UnitPriceExclTax,
                   UnitPriceInclTaxValue = item.UnitPriceInclTax,
                   MinPrice = item.MinPrice,
                   OldPrice = item.OldPrice,
                   // OldPrice = item.OldPriceIncTax,
                   DiscountExclTax = item.DiscountAmountExclTax,
                   Quantity = item.Quantity,
                   StockQuantity = item.StockQuantity,
                   LastPrice = lastPrices.FirstOrDefault(p => p.ProductId == item.ProductId)?.UnitPriceExclTax ?? item.UnitPriceExclTax,
                   TotalExclTax = (item.Quantity * item.UnitPriceExclTax) - item.DiscountAmountExclTax,
                   TotalInclTax = (item.Quantity * item.UnitPriceInclTax) - item.DiscountAmountExclTax,
                   PurchasePrice = item.PurchasePrice
               }).ToList();
                if (orderItems != null && orderItems.Count > 0)
                    SelectedQuickOrderItems.AddRange(orderItems.ToList());
                // Maintain Session

            }// Existing Quick Order            
            _httpContext.HttpContext.Session.SetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid, JsonConvert.SerializeObject(SelectedQuickOrderItems));
            return Json(SelectedQuickOrderItems.ToList());
        }
        public virtual IActionResult GetCartItemWithLastPrice(int customerId)
        {
            OrderModel model = null;
            var searchModel = new CustomerShoppingCartSearchModel();
            searchModel.CustomerId = customerId;
            searchModel.ShoppingCartTypeId = 1;
            searchModel.SetGridPageSize(1000);
            var customer = _customerService.GetCustomerById(customerId);
            var cartList = _customerModelFactory.PrepareCustomerShoppingCartListModel(searchModel, customer);
            var order = new Order();
            order.Customer = customer;
            order.CustomerId = customerId;
            order.OrderStatusId = (int)OrderStatus.Pending;
            order.BillingAddress = customer.BillingAddress;
            order.BillingAddressId = customer.BillingAddressId ?? 0;
            order.ShippingAddress = customer.ShippingAddress;
            order.ShippingAddressId = customer.ShippingAddressId ?? 0;
            //a vendor should have access only to his products
            foreach (var i in cartList.Data)
            {
                var orderItem = new OrderItem();
                var product = _productService.GetProductById(i.ProductId);
                orderItem.ProductId = i.ProductId;
                orderItem.Product = product;
                orderItem.UnitPriceExclTax = i.UnitPriceDec;
                orderItem.Quantity = i.Quantity;
                order.OrderItems.Add(orderItem);
            }
            model = new OrderModel();
            model.SalemanId = customer.TeamMemberOfSaleman;
            model.CustomerId = customerId;
            model.OrderStatusId = (int)OrderStatus.Pending;
            model = _orderModelFactory.PrepareNewQuickOrderModel(model, true);
            model.order = order;
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }
            String json = _httpContext.HttpContext.Session.GetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid);
            if (!string.IsNullOrEmpty(json))
            {
                SelectedQuickOrderItems = JsonConvert.DeserializeObject<List<QuickOrderItemModel>>(json);
            }
            if (string.IsNullOrEmpty(json) && order != null)
            {
                //var order = _orderService.GetOrderById(Id);
                var productIds = order.OrderItems.Select(x => x.ProductId).ToArray();
                var lastPrices = _orderService.GetLastPricesByCustomer(customerId, productIds);
                try
                {
                    // Get Existing Order item List
                    var orderItems = (from oItem in order.OrderItems
                                      select new
                                      {
                                          ProductId = oItem.ProductId,
                                          ProductName = oItem.Product.Name,
                                          Sku = oItem.Product.Sku,
                                          MinPrice = oItem.Product.MinPrice,
                                          OldPrice = oItem.Product.OldPrice,
                                          //  OldPriceIncTax = oItem.OldPrice,
                                          UnitPriceExclTax = oItem.UnitPriceExclTax,
                                          UnitPriceInclTax = _taxService.GetProductPrice(oItem.Product, oItem.UnitPriceExclTax, true, order.Customer, out _),
                                          DiscountAmountExclTax = oItem.DiscountAmountExclTax,
                                          Quantity = oItem.Quantity,
                                          StockQuantity = oItem.Product.StockQuantity,
                                          PurchasePrice = oItem.Product.ProductCost

                                      })
                   .OrderByDescending(x => x.ProductName);
                    var quickOrderItems = new List<QuickOrderItemModel>();
                    foreach (var item in orderItems)
                    {
                        var lastPrice = lastPrices.FirstOrDefault(p => p.ProductId == item.ProductId)?.UnitPriceExclTax ?? 0;
                        var qi = new QuickOrderItemModel
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            Sku = item.Sku,
                            UnitPriceExclTax = item.UnitPriceExclTax,
                            UnitPriceInclTaxValue = item.UnitPriceInclTax,
                            MinPrice = item.MinPrice,
                            OldPrice = item.OldPrice,
                            // OldPrice = item.OldPriceIncTax,
                            DiscountExclTax = item.DiscountAmountExclTax,
                            Quantity = item.Quantity,
                            StockQuantity = item.StockQuantity,
                            LastPrice = lastPrice,
                            TotalExclTax = (item.Quantity * item.UnitPriceExclTax) - item.DiscountAmountExclTax,
                            TotalInclTax = (item.Quantity * item.UnitPriceInclTax) - item.DiscountAmountExclTax,
                            PurchasePrice = item.PurchasePrice
                        };
                        quickOrderItems.Add(qi);
                    }
                    if (orderItems != null && quickOrderItems.Count > 0)
                        SelectedQuickOrderItems.AddRange(quickOrderItems.ToList());
                    // Maintain Session
                }
                catch (Exception ex)
                {

                }


            }// Existing Quick Order            
            _httpContext.HttpContext.Session.SetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid, JsonConvert.SerializeObject(SelectedQuickOrderItems));
            return Json(SelectedQuickOrderItems.ToList());
        }
        // OrderItems
        public virtual IActionResult GetOrderItems(int Id)
        {

            //a vendor should have access only to his products
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }
            String json = _httpContext.HttpContext.Session.GetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid);
            if (!string.IsNullOrEmpty(json))
            {
                SelectedQuickOrderItems = JsonConvert.DeserializeObject<List<QuickOrderItemModel>>(json);
            }
            if (string.IsNullOrEmpty(json) && Id > 0)
            {
                var order = _orderService.GetOrderById(Id);

                // Get Existing Order item List
                var orderItems = (from oItem in order.OrderItems
                                  select new
                                  {
                                      ProductId = oItem.ProductId,
                                      ProductName = oItem.Product.Name,
                                      Sku = oItem.Product.Sku,
                                      MinPrice = oItem.Product.MinPrice,
                                      //  OldPriceIncTax = oItem.OldPrice,
                                      UnitPriceExclTax = oItem.UnitPriceExclTax,
                                      UnitPriceInclTax = _taxService.GetProductPrice(oItem.Product, oItem.UnitPriceExclTax, true, order.Customer, out _),
                                      DiscountAmountExclTax = oItem.DiscountAmountExclTax,
                                      Quantity = oItem.Quantity,
                                      StockQuantity = oItem.Product.StockQuantity
                                  })
               .OrderByDescending(x => x.ProductName)
               .Select(item => new QuickOrderItemModel
               {
                   ProductId = item.ProductId,
                   ProductName = item.ProductName,
                   Sku = item.Sku,
                   UnitPriceExclTax = item.UnitPriceExclTax,
                   UnitPriceInclTaxValue = item.UnitPriceInclTax,
                   MinPrice = item.MinPrice,
                   // OldPrice = item.OldPriceIncTax,
                   DiscountExclTax = item.DiscountAmountExclTax,
                   Quantity = item.Quantity,
                   StockQuantity = item.StockQuantity,
                   TotalExclTax = (item.Quantity * item.UnitPriceExclTax) - item.DiscountAmountExclTax,
                   TotalInclTax = (item.Quantity * item.UnitPriceInclTax) - item.DiscountAmountExclTax,
               }).ToList();
                if (orderItems != null && orderItems.Count > 0)
                    SelectedQuickOrderItems.AddRange(orderItems.ToList());
                // Maintain Session

            }// Existing Quick Order            
            _httpContext.HttpContext.Session.SetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid, JsonConvert.SerializeObject(SelectedQuickOrderItems));
            return Json(SelectedQuickOrderItems.ToList());
        }
        public virtual QuickOrderItemModel OrderItemAdd(QuickOrderItemModel OrderItem)
        {
            String json = _httpContext.HttpContext.Session.GetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid);
            if (!string.IsNullOrEmpty(json))
            {
                SelectedQuickOrderItems = JsonConvert.DeserializeObject<List<QuickOrderItemModel>>(json);
            }

            //var QuickOrderItemModel = OrderItem.ToEntity < QuickOrderItemModel >
            if (SelectedQuickOrderItems.Where(x => x.ProductId == OrderItem.ProductId).FirstOrDefault() == null)
                SelectedQuickOrderItems.Add(OrderItem);

            _httpContext.HttpContext.Session.SetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid, JsonConvert.SerializeObject(SelectedQuickOrderItems));
            return OrderItem;
        }
        public virtual QuickOrderItemModel OrderItemUpdate(QuickOrderItemModel OrderItem)
        {
            String json = _httpContext.HttpContext.Session.GetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid);
            if (!string.IsNullOrEmpty(json))
            {
                SelectedQuickOrderItems = JsonConvert.DeserializeObject<List<QuickOrderItemModel>>(json);
            }

            // if (_httpContext.Session.Get("OrderItems" + _workContext.CurrentCustomer.CustomerGuid) != null)
            // var  SelectedQuickOrderItems = _httpContext.Session.Get("OrderItems" + _workContext.CurrentCustomer.CustomerGuid);

            QuickOrderItemModel item = SelectedQuickOrderItems.Where(x => x.ProductId == OrderItem.ProductId).FirstOrDefault();
            if (item != null)
            {
                item.Quantity = OrderItem.Quantity;
                item.TotalExclTax = OrderItem.TotalExclTax;
                item.TotalInclTax = OrderItem.TotalInclTax;
                item.UnitPriceExclTax = OrderItem.UnitPriceExclTax;
                item.MinPrice = OrderItem.MinPrice;
                item.DiscountExclTax = OrderItem.DiscountExclTax;

            }
            _httpContext.HttpContext.Session.SetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid, JsonConvert.SerializeObject(SelectedQuickOrderItems));
            return OrderItem;

        }
        public virtual IActionResult OrderItemDelete(int ProductId)
        {
            String json = _httpContext.HttpContext.Session.GetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid);
            if (!string.IsNullOrEmpty(json))
            {
                SelectedQuickOrderItems = JsonConvert.DeserializeObject<List<QuickOrderItemModel>>(json);
            }
            // if (_httpContext.Session.Get("OrderItems" + _workContext.CurrentCustomer.CustomerGuid) != null)
            // SelectedQuickOrderItems = _httpContext.Session.Get("OrderItems" + _workContext.CurrentCustomer.CustomerGuid);
            QuickOrderItemModel item = SelectedQuickOrderItems.Where(x => x.ProductId == ProductId).FirstOrDefault();
            if (item != null)
            {
                SelectedQuickOrderItems.Remove(item);
                _httpContext.HttpContext.Session.SetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid, JsonConvert.SerializeObject(SelectedQuickOrderItems));
            }
            return Json(1);
        }
        public virtual IActionResult ProductGetAll()
        {

            //a vendor should have access only to his products
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            //products
            const int productNumber = 10000;
            var products = _productService.SearchProducts(
                vendorId: vendorId,
                keywords: "",
                pageSize: productNumber,
                showHidden: false);
            var result = (from p in products
                          select new
                          {
                              ProductName = p.Name,
                              StockQuantity = p.StockQuantity,
                              Price = p.Price,
                              OldPrice = p.OldPrice,
                              Sku = p.Sku,
                              ProductId = p.Id
                          }).ToList();
            return Json(result);
        }
        public virtual IActionResult ProductGetAllWithLastPrice(int customerId)
        {

            //a vendor should have access only to his products
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }
            var customer = _customerService.GetCustomerById(customerId) ?? new Core.Domain.Customers.Customer();
            //products
            const int productNumber = 10000;
            var products = _productService.SearchProducts(
                vendorId: vendorId,
                keywords: "",
                pageSize: productNumber,
                showHidden: false);
            var ids = products.Select(x => x.Id);
            var lastPrices = _orderService.GetLastPricesByCustomer(customerId);
            var customerTierPrices = _productService.GeCustomerTierPrices(customer);
            var result = (from p in products
                          select new
                          {
                              ProductName = p.Name + ", " + (p.Id + 10000) + ", " + p.Sku + ", " + p.ManufacturerPartNumber,
                              StockQuantity = p.StockQuantity,
                              Price = customerTierPrices.Where(t => t.ProductId == p.Id).FirstOrDefault()?.Price ?? p.Price,
                              OldPrice = p.OldPrice,
                              MinPrice = p.MinPrice,
                              Sku = p.Sku,
                              NewStockComing = p.NewStockComing,
                              ProductId = p.Id,
                              LastPrice = lastPrices.Any(x => x.ProductId == p.Id) ? lastPrices.FirstOrDefault(x => x.ProductId == p.Id).UnitPriceExclTax : 0,
                              PurchasePrice = p.ProductCost
                          }).ToList();
            var jResult = Json(result);
            return jResult;
        }
        public virtual IActionResult ProductSearchAutoComplete(string term)
        {
            const int searchTermMinimumLength = 3;
            if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content(string.Empty);

            //a vendor should have access only to his products
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            //products
            const int productNumber = 15;
            var products = _productService.SearchProducts(
                vendorId: vendorId,
                keywords: term,
                pageSize: productNumber,
                showHidden: true);

            var result = (from p in products
                          select new
                          {
                              label = p.Name,
                              productid = p.Id
                          }).ToList();
            return Json(result);
        }

        #endregion

        #region Export / Import

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public virtual IActionResult ExportXmlAll(OrderSearchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var startDateValue = model.StartDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            var endDateValue = model.EndDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            var orderStatusIds = model.OrderStatusIds != null && !model.OrderStatusIds.Contains(0)
                ? model.OrderStatusIds.ToList()
                : null;
            var paymentStatusIds = model.PaymentStatusIds != null && !model.PaymentStatusIds.Contains(0)
                ? model.PaymentStatusIds.ToList()
                : null;
            var shippingStatusIds = model.ShippingStatusIds != null && !model.ShippingStatusIds.Contains(0)
                ? model.ShippingStatusIds.ToList()
                : null;

            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && (_workContext.CurrentVendor == null || product.VendorId == _workContext.CurrentVendor.Id))
                filterByProductId = model.ProductId;

            //load orders
            var orders = _orderService.SearchOrders(storeId: model.StoreId,
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingPhone: model.BillingPhone,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes);

            try
            {
                var xml = _exportManager.ExportOrdersToXml(orders);

                return File(Encoding.UTF8.GetBytes(xml), MimeTypes.ApplicationXml, "orders.xml");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual IActionResult ExportXmlSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(_orderService.GetOrdersByIds(ids).Where(HasAccessToOrder));
            }

            var xml = _exportManager.ExportOrdersToXml(orders);

            return File(Encoding.UTF8.GetBytes(xml), MimeTypes.ApplicationXml, "orders.xml");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public virtual IActionResult ExportExcelAll(OrderSearchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var startDateValue = model.StartDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            var endDateValue = model.EndDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            var orderStatusIds = model.OrderStatusIds != null && !model.OrderStatusIds.Contains(0)
                ? model.OrderStatusIds.ToList()
                : null;
            var paymentStatusIds = model.PaymentStatusIds != null && !model.PaymentStatusIds.Contains(0)
                ? model.PaymentStatusIds.ToList()
                : null;
            var shippingStatusIds = model.ShippingStatusIds != null && !model.ShippingStatusIds.Contains(0)
                ? model.ShippingStatusIds.ToList()
                : null;

            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && (_workContext.CurrentVendor == null || product.VendorId == _workContext.CurrentVendor.Id))
                filterByProductId = model.ProductId;

            //load orders
            var orders = _orderService.SearchOrders(storeId: model.StoreId,
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingPhone: model.BillingPhone,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes);

            try
            {
                var bytes = _exportManager.ExportOrdersToXlsx(orders);
                return File(bytes, MimeTypes.TextXlsx, "orders.xlsx");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual IActionResult ExportExcelSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(_orderService.GetOrdersByIds(ids).Where(HasAccessToOrder));
            }

            try
            {
                var bytes = _exportManager.ExportOrdersToXlsx(orders);
                return File(bytes, MimeTypes.TextXlsx, "orders.xlsx");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        #endregion

        #region Order details

        #region Payments and other order workflow

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelorder")]
        public virtual IActionResult CancelOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                _orderProcessingService.CancelOrder(order, true);
                LogEditOrder(order.Id);

                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("captureorder")]
        public virtual IActionResult CaptureOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                var errors = _orderProcessingService.Capture(order);
                LogEditOrder(order.Id);

                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                foreach (var error in errors)
                    _notificationService.ErrorNotification(error);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markorderaspaid")]
        public virtual IActionResult MarkOrderAsPaid(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                _orderProcessingService.MarkOrderAsPaid(order);
                LogEditOrder(order.Id);

                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markorderasunpaid")]
        public virtual IActionResult MarkOrderAsUnPaid(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                _orderProcessingService.MarkOrderAsUnPaid(order);
                LogEditOrder(order.Id);

                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorder")]
        public virtual IActionResult RefundOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                var errors = _orderProcessingService.Refund(order);
                LogEditOrder(order.Id);

                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                foreach (var error in errors)
                    _notificationService.ErrorNotification(error);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorderoffline")]
        public virtual IActionResult RefundOrderOffline(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                _orderProcessingService.RefundOffline(order);
                LogEditOrder(order.Id);

                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorder")]
        public virtual IActionResult VoidOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                var errors = _orderProcessingService.Void(order);
                LogEditOrder(order.Id);

                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                foreach (var error in errors)
                    _notificationService.ErrorNotification(error);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorderoffline")]
        public virtual IActionResult VoidOrderOffline(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                _orderProcessingService.VoidOffline(order);
                LogEditOrder(order.Id);

                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        public virtual IActionResult PartiallyRefundOrderPopup(int id, bool online)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            //prepare model
            var model = _orderModelFactory.PrepareOrderModel(null, order);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("partialrefundorder")]
        public virtual IActionResult PartiallyRefundOrderPopup(int id, bool online, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                var amountToRefund = model.AmountToRefund;
                if (amountToRefund <= decimal.Zero)
                    throw new NopException("Enter amount to refund");

                var maxAmountToRefund = order.OrderTotal - order.RefundedAmount;
                if (amountToRefund > maxAmountToRefund)
                    amountToRefund = maxAmountToRefund;

                var errors = new List<string>();
                if (online)
                    errors = _orderProcessingService.PartiallyRefund(order, amountToRefund).ToList();
                else
                    _orderProcessingService.PartiallyRefundOffline(order, amountToRefund);

                LogEditOrder(order.Id);

                if (!errors.Any())
                {
                    //success
                    ViewBag.RefreshPage = true;

                    //prepare model
                    model = _orderModelFactory.PrepareOrderModel(model, order);

                    return View(model);
                }

                //prepare model
                model = _orderModelFactory.PrepareOrderModel(model, order);

                foreach (var error in errors)
                    _notificationService.ErrorNotification(error);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                model = _orderModelFactory.PrepareOrderModel(model, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderStatus")]
        public virtual IActionResult ChangeOrderStatus(int id, OrderModel model)
        {
            //var orders = _orderService.GetAllOrders();
            //if ((orders[0].Profit ?? 0) == 0)
            //{
            //    foreach (var o in orders)
            //    {
            //        _orderService.UpdateOrder(o,true);
            //    }
            //}

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                order.OrderStatusId = model.OrderStatusId;
                //if (order.OrderStatusId == (int)OrderStatus.Complete && model.OrderStatusId != (int)OrderStatus.Complete)
                {
                    order.GrossProfit = _orderReportService.ProfitReport(orderId: order.Id);
                    order.Profit = _orderReportService.NetProfitReport(orderId: order.Id);
                    if (order.SalemanId == 0)
                    {
                        var customer = _customerService.GetCustomerById(order.CustomerId);
                        if (customer != null)
                        {
                            order.SalemanId = customer.TeamMemberOfSaleman;
                        }
                    }
                    _customerService.CalculateSalemanCommission(order.SalemanId, order.CreatedOnUtc);
                }
                _orderService.UpdateOrder(order);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = $"Order status has been edited. New status: {_localizationService.GetLocalizedEnum(order.OrderStatus)}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                //prepare model
                model = _orderModelFactory.PrepareOrderModel(model, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                model = _orderModelFactory.PrepareOrderModel(model, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveActualShippingCost")]
        public virtual IActionResult ChangeOrderActualShippingCost(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                order.ActualShippingCost = model.ActualShippingCost;
                //if (order.OrderStatusId == (int)OrderStatus.Complete && model.OrderStatusId != (int)OrderStatus.Complete)
                {
                    order.GrossProfit = _orderReportService.ProfitReport(orderId: order.Id);
                    order.Profit = _orderReportService.NetProfitReport(orderId: order.Id);
                    _customerService.CalculateSalemanCommission(order.SalemanId, order.CreatedOnUtc);
                }
                _orderService.UpdateOrder(order);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = $"Order status has been edited. New status: {_localizationService.GetLocalizedEnum(order.OrderStatus)}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                //prepare model
                model = _orderModelFactory.PrepareOrderModel(model, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                model = _orderModelFactory.PrepareOrderModel(model, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveNoteForWarehouse", "btnSaveNoteForAccounts")]
        public virtual IActionResult ChangeOrderNotes(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                order.NoteForWarehouse = model.NoteForWarehouse;
                order.NoteForAccounts = model.NoteForAccounts;
                _orderService.UpdateOrder(order);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = $"Order status has been edited. New status: {_localizationService.GetLocalizedEnum(order.OrderStatus)}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                //prepare model
                model = _orderModelFactory.PrepareOrderModel(model, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                model = _orderModelFactory.PrepareOrderModel(model, order);

                _notificationService.ErrorNotification(exc);
                return View(model);
            }
        }

        #endregion

        #region Edit, delete

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null && !HasAccessToOrder(order))
                return RedirectToAction("List");

            //prepare model
            var model = _orderModelFactory.PrepareOrderModel(null, order);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult QuickOrderSubmit(OrderModel model)
        {
            //if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
            //    return AccessDeniedView();
            var customer = _customerService.GetCustomerById(model.CustomerId);
            try
            {
                String json = _httpContext.HttpContext.Session.GetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid);
                if (!string.IsNullOrEmpty(json))
                {
                    SelectedQuickOrderItems = JsonConvert.DeserializeObject<List<QuickOrderItemModel>>(json);
                }
                model.IsQuickOrder = true;
                // Two Tracks 1- New Quick order, 2- Eidt Quick Order
                if (model.Id == 0)
                {
                    ProcessPaymentRequest ppr = new ProcessPaymentRequest();
                    ppr.CustomerId = model.CustomerId;
                    ppr.ShippingAddressId = model.ShippingAddressId;
                    ppr.SalesmanId = model.SalemanId;
                    ppr.PaymentMethodSystemName = model.PaymentMethod;
                    ppr.OrderGuid = model.OrderGuid;
                    ppr.OrderGuidGeneratedOnUtc = model.DateInvoice;
                    ppr.StoreId = customer.RegisteredInStoreId;
                    ppr.ReferneceNo = model.ReferneceNo;

                    var orderItems = (from oItem in SelectedQuickOrderItems
                                      select oItem
                                     )
                  .OrderByDescending(x => x.ProductName)
                  .Select(item => new OrderItem
                  {
                      ProductId = item.ProductId,
                      DiscountAmountExclTax = item.DiscountExclTax,
                      DiscountAmountInclTax = item.DiscountInclTax,
                      UnitPriceExclTax = item.UnitPriceExclTax,
                      UnitPriceInclTax = item.UnitPriceInclTaxValue,
                      AttributesXml = null,
                      Product = _productService.GetProductById(item.ProductId),
                      Quantity = item.Quantity,
                      PriceExclTax = item.TotalExclTax,
                      PriceInclTax = item.TotalInclTax,
                  }).ToList();

                    // Get Order Items
                    PlaceOrderResult por = _orderProcessingService.PlaceQuickOrder(ppr, orderItems.ToList(), orderStatus: getOrderStatusEnum(model.OrderStatusId));

                    if (por.Success)
                    {
                        var quickOrder = por.PlacedOrder;
                        model.OrderStatusId = quickOrder.OrderStatusId;
                        model.OrderStatus = getOrderStatusEnum(quickOrder.OrderStatusId).ToString();
                        model.OrderStatusId = quickOrder.OrderStatusId;
                        model.DateInvoice = quickOrder.CreatedOnUtc;
                        model.PaymentTermId = quickOrder.PaymentTermId;
                        model.Id = quickOrder.Id;
                        quickOrder.GrossProfit = _orderReportService.ProfitReport(orderId: model.Id);
                        quickOrder.Profit = _orderReportService.NetProfitReport(orderId: model.Id);
                        quickOrder.ActualShippingCost = model.CustomShippingCharges != null ? model.CustomShippingCharges.Value : 0;
                        quickOrder.SalemanId = customer.TeamMemberOfSaleman;
                        if (model.CustomShippingCharges != null)
                        {
                            quickOrder.OrderShippingExclTax = model.CustomShippingCharges.Value;
                            quickOrder.OrderShippingInclTax = _taxService.GetShippingPrice(model.CustomShippingCharges.Value, true, quickOrder.Customer);
                            quickOrder.CustomShippingCharges = model.CustomShippingCharges;
                            quickOrder.OrderTotal = quickOrder.OrderSubtotalInclTax + quickOrder.OrderShippingInclTax + quickOrder.PaymentMethodAdditionalFeeInclTax;
                            _orderService.UpdateOrder(quickOrder);
                        }
                        //add a note
                        if (!string.IsNullOrEmpty(model.Remarks))
                        {
                            quickOrder.OrderNotes.Add(new OrderNote
                            {
                                Note = "" + model.Remarks,
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            //*****************************  SET Calender Date *********************************/
                            if (model.DateInvoice != null)
                                quickOrder.CreatedOnUtc = Convert.ToDateTime(model.DateInvoice);
                            _orderService.UpdateOrder(quickOrder);
                        }
                        SelectedQuickOrderItems.Clear();
                        _httpContext.HttpContext.Session.SetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid, JsonConvert.SerializeObject(SelectedQuickOrderItems));

                        _notificationService.SuccessNotification(string.Format(_localizationService.GetResource("Admin.QuickOrder.PlaceOrder.Success"), quickOrder.CustomOrderNumber, _priceFormatter.FormatPrice(quickOrder.OrderTotal, true, false), quickOrder.CreatedOnUtc.ToString()));
                        return RedirectToAction("Edit", new { id = model.Id });
                    }
                    else
                    {
                        _notificationService.ErrorNotification(_localizationService.GetResource("Admin.QuickOrder.PlaceOrder.Error") + " , " + por.Errors[0].ToString());
                        return RedirectToAction("QuickOrder", "Order", new { id = model.Id, SalemanId = model.SalemanId, customerId = model.CustomerId });
                    }
                }//  New Quick Order
                else
                {
                    //try to get an order with the specified id
                    var order = _orderService.GetOrderById(model.Id);
                    var reCalc = !order.Profit.HasValue || order.Profit == 0 || order.OrderShippingInclTax != model.CustomShippingCharges;
                    order.IsQuickOrder = true;

                    OrderStatus OldStatus = order.OrderStatus;
                    if (order == null)
                        return RedirectToAction("QuickOrder", new { id = model.Id, SalemanId = model.SalemanId, customerId = model.CustomerId });

                    //a vendor does not have access to this functionality
                    if ((order.OrderStatus == OrderStatus.Pending || order.OrderStatus == OrderStatus.Processing || order.OrderStatus == OrderStatus.Cancelled || order.OrderStatus == OrderStatus.Complete) && (model.OrderStatusId == (int)OrderStatus.Pending || model.OrderStatusId == (int)OrderStatus.BackOrder) && (model.OrderStatusId != order.OrderStatusId))
                    {
                        _notificationService.ErrorNotification(_localizationService.GetResource("Admin.QuickOrder.PlaceOrder.ProcessError"));
                        return RedirectToAction("QuickOrder", new { id = model.Id, SalemanId = model.SalemanId, customerId = model.CustomerId });
                    }
                    //***************************** Order Level Attributes*********************************************************/

                    order.OrderStatus = getOrderStatusEnum(model.OrderStatusId);
                    order.SalemanId = model.SalemanId;
                    order.CustomerId = model.CustomerId;
                    order.ShippingAddressId = model.ShippingAddressId;
                    order.PaymentMethodSystemName = model.PaymentMethod;
                    order.OrderStatusId = model.OrderStatusId;
                    order.PaymentTermId = model.PaymentTermId;
                    if (model.CustomShippingCharges != null)
                    {
                        order.OrderShippingExclTax = model.CustomShippingCharges.Value;
                        order.OrderShippingInclTax = _taxService.GetShippingPrice(model.CustomShippingCharges.Value, true, order.Customer);
                        order.CustomShippingCharges = model.CustomShippingCharges;
                        order.OrderTotal = order.OrderSubtotalInclTax + order.OrderShippingInclTax;
                    }
                    order.OrderTotal += order.PaymentMethodAdditionalFeeInclTax;
                    order.ReferneceNo = model.ReferneceNo;
                    // Get All Order Order Items for further Process
                    //****************************************** Prepare Order Items*************************************************/
                    var orderItems = (from oItem in SelectedQuickOrderItems
                                      select oItem
                                    )
                 .OrderByDescending(x => x.ProductName)
                 .Select(item => new OrderItem
                 {
                     OrderItemGuid = Guid.NewGuid(),
                     ProductId = item.ProductId,
                     DiscountAmountExclTax = item.DiscountExclTax,
                     DiscountAmountInclTax = item.DiscountInclTax,
                     UnitPriceExclTax = item.UnitPriceExclTax,
                     Product = _productService.GetProductById(item.ProductId),
                     UnitPriceInclTax = _taxService.GetProductPrice(_productService.GetProductById(item.ProductId), item.UnitPriceExclTax, true, order.Customer, out _),
                     //OldPriceExclTax = item.OldPriceExclTax,
                     //OldPriceIncTax = item.OldPriceInclTax,
                     AttributesXml = null,
                     Quantity = item.Quantity,
                     PriceExclTax = item.TotalExclTax,
                     PriceInclTax = _taxService.GetProductPrice(_productService.GetProductById(item.ProductId), item.TotalExclTax, true, order.Customer, out _)
                 }).ToList();
                    //****************************************** CALL  Order Processing Service for Update*************************************************/
                    PlaceOrderResult por = _orderProcessingService.UpdateQuickOrder(order, orderItems.ToList(), OldStatus);

                    if (por.Success)
                    {
                        //add a note
                        if (!string.IsNullOrEmpty(model.Remarks))
                        {
                            order.OrderNotes.Add(new OrderNote
                            {
                                Note = "" + model.Remarks,
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            //*****************************  SET Calender Date *********************************/
                            if (model.DateInvoice != null)
                            {
                                _orderService.UpdateOrder(order);
                                if (reCalc)
                                {
                                    var savedOrder = _orderService.GetOrderById(model.Id);
                                    decimal? netProfit = order.Profit;
                                    decimal? grossProfit = order.GrossProfit;
                                    netProfit = _orderReportService.NetProfitReport(orderId: savedOrder.Id);
                                    grossProfit = _orderReportService.ProfitReport(orderId: savedOrder.Id);
                                    savedOrder.Profit = netProfit;
                                    savedOrder.GrossProfit = grossProfit;
                                    _orderService.UpdateOrder(savedOrder);
                                }
                            }
                        }
                        SelectedQuickOrderItems.Clear();
                        _httpContext.HttpContext.Session.SetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid, JsonConvert.SerializeObject(SelectedQuickOrderItems));

                        _notificationService.SuccessNotification(string.Format(_localizationService.GetResource("Admin.QuickOrder.PlaceOrder.UpdateSuccess"), order.CustomOrderNumber, System.DateTime.Now.ToString()));
                    }
                    else
                    {
                        _notificationService.ErrorNotification(_localizationService.GetResource("Admin.QuickOrder.PlaceOrder.Error") + " , " + por.Errors[0].ToString());
                    }

                    //prepare model
                    model = _orderModelFactory.PrepareOrderModel(model, order);
                    model.SalemanId = order.SalemanId;
                    model.OrderStatusId = order.OrderStatusId;
                    model.CustomerId = order.CustomerId;
                    model.DateInvoice = order.CreatedOnUtc;
                    model.DateInvoice = order.CreatedOnUtc;
                    model.ReferneceNo = order.ReferneceNo;
                    return RedirectToAction("Edit", new { id = model.Id });
                    //return RedirectToAction("QuickOrder", "Order", new { id = model.Id, SalemanId = model.SalemanId, customerId = model.CustomerId });
                    // return View(model);

                }
            }
            catch (Exception ex)
            {
                // Error Notification
                _notificationService.ErrorNotification(ex.Message);
                return RedirectToAction("QuickOrder");
            }
            //return Json(true);
        }

        private OrderStatus getOrderStatusEnum(int statusId)
        {
            OrderStatus oStatus = OrderStatus.Pending;
            switch (statusId)
            {
                case (int)OrderStatus.Pending:
                    oStatus = OrderStatus.Pending;
                    break;
                case (int)OrderStatus.Cancelled:
                    oStatus = OrderStatus.Cancelled;
                    break;
                case (int)OrderStatus.Processing:
                    oStatus = OrderStatus.Processing;
                    break;
                case (int)OrderStatus.Complete:
                    oStatus = OrderStatus.Complete;
                    break;
                case (int)OrderStatus.BackOrder:
                    oStatus = OrderStatus.BackOrder;
                    break;
                case (int)OrderStatus.Offer:
                    oStatus = OrderStatus.Offer;
                    break;
            }
            return oStatus;
        }
        public virtual IActionResult QuickOrder(int id, int customerId, int SalemanId, int cart)
        {
            this.SelectedQuickOrderItems.Clear();
            _httpContext.HttpContext.Session.SetString("OrderItems" + _workContext.CurrentCustomer.CustomerGuid, string.Empty);

            OrderModel model = null;
            try
            {
                if (cart == 1)
                {
                    model = _orderModelFactory.PrepareNewQuickOrderModel(model, true);
                    var customer = _customerService.GetCustomerById(customerId);
                    model.SalemanId = customer.TeamMemberOfSaleman;
                    model.CustomerId = customerId;
                    model.cart = 1;
                    //var searchModel = new CustomerShoppingCartSearchModel();
                    //searchModel.CustomerId = customerId;
                    //searchModel.ShoppingCartTypeId = 1;
                    //var customer = _customerService.GetCustomerById(customerId);
                    //var cartList = _customerModelFactory.PrepareCustomerShoppingCartListModel(searchModel, customer);
                    //var order = new Order();
                    //order.Customer = customer;
                    //order.CustomerId = customerId;
                    //order.OrderStatusId = (int)OrderStatus.Pending;
                    //order.BillingAddress = customer.BillingAddress;
                    //order.BillingAddressId = customer.BillingAddressId ?? 0;
                    //order.ShippingAddress = customer.ShippingAddress;
                    //order.ShippingAddressId = customer.ShippingAddressId ?? 0;
                    ////model = _orderModelFactory.PrepareNewQuickOrderModel(model, true);
                    ////model.SalemanId = customer.TeamMemberOfSaleman;
                    ////model.CustomerId = customerId;
                    ////model.OrderStatusId = (int)OrderStatus.Pending;
                    //foreach (var i in cartList.Data)
                    //{
                    //    var orderItem = new OrderItem();
                    //    var product = _productService.GetProductById(i.ProductId);
                    //    orderItem.ProductId = i.ProductId;
                    //    orderItem.Product = product;
                    //    order.OrderItems.Add(orderItem);
                    //}
                    //model = new OrderModel();
                    //model.SalemanId = customer.TeamMemberOfSaleman;
                    //model.CustomerId = customerId;
                    //model.OrderStatusId = (int)OrderStatus.Pending;
                    //model = _orderModelFactory.PrepareNewQuickOrderModel(model, true);
                    //model = _orderModelFactory.PrepareQuickOrderModel(model, order);
                    //order.OrderGuid = model.order.OrderGuid;

                    //model.order = order;
                }
                else if (id > 0)
                {
                    var order = _orderService.GetOrderById(id);
                    order.IsQuickOrder = true;
                    model = _orderModelFactory.PrepareQuickOrderModel(model, order);
                    model.IsQuickOrder = true;
                    model.OrderStatusId = order.OrderStatusId;
                    // Last Note 
                    var orderNote = order.OrderNotes.OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault();
                    model.Remarks = orderNote != null ? orderNote.Note : "";
                    if (SalemanId == 0 && customerId == 0)
                    {
                        SalemanId = order.SalemanId;
                        customerId = order.CustomerId;
                    }
                    model.SalemanId = SalemanId;
                    model.PaymentTermId = order.PaymentTermId;
                    model.CustomerId = customerId;
                    model.ShippingAddressId = order.ShippingAddressId ?? 0;
                    model.ReferneceNo = order.ReferneceNo;
                    model.CustomShippingCharges = order.OrderShippingExclTax;
                    model.cart = 0;
                }
                else
                {
                    model = _orderModelFactory.PrepareNewQuickOrderModel(model, true);
                    model.CustomerId = 0;
                    model.cart = 0;
                }
                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model 
                return View(model);
            }
            ////try to get an order with the specified id
            //var order = _orderService.GetOrderById(5);
            //if (order == null || order.Deleted)
            //    return RedirectToAction("QuickOrder");

            ////a vendor does not have access to this functionality
            ////if (_workContext.CurrentVendor != null && !HasAccessToOrder(order))
            ////    return RedirectToAction("QuickOrder");

            ////prepare model
            //


        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            _orderProcessingService.DeleteOrder(order);

            //activity log
            _customerActivityService.InsertActivity("DeleteOrder",
                string.Format(_localizationService.GetResource("ActivityLog.DeleteOrder"), order.Id), order);

            return RedirectToAction("List");
        }

        public virtual IActionResult PdfInvoice(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor should have access only to his products
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            var order = _orderService.GetOrderById(orderId);
            var orders = new List<Order>
            {
                order
            };

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintOrdersToPdf(stream, orders, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id, vendorId);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, $"order_{order.Id}.pdf");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("pdf-invoice-all")]
        public virtual IActionResult PdfInvoiceAll(OrderSearchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
            }

            var startDateValue = model.StartDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            var endDateValue = model.EndDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var orderStatusIds = model.OrderStatusIds != null && !model.OrderStatusIds.Contains(0)
                ? model.OrderStatusIds.ToList()
                : null;
            var paymentStatusIds = model.PaymentStatusIds != null && !model.PaymentStatusIds.Contains(0)
                ? model.PaymentStatusIds.ToList()
                : null;
            var shippingStatusIds = model.ShippingStatusIds != null && !model.ShippingStatusIds.Contains(0)
                ? model.ShippingStatusIds.ToList()
                : null;

            var filterByProductId = 0;
            var product = _productService.GetProductById(model.ProductId);
            if (product != null && (_workContext.CurrentVendor == null || product.VendorId == _workContext.CurrentVendor.Id))
                filterByProductId = model.ProductId;

            //load orders
            var orders = _orderService.SearchOrders(storeId: model.StoreId,
                vendorId: model.VendorId,
                productId: filterByProductId,
                warehouseId: model.WarehouseId,
                paymentMethodSystemName: model.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingPhone: model.BillingPhone,
                billingEmail: model.BillingEmail,
                billingLastName: model.BillingLastName,
                billingCountryId: model.BillingCountryId,
                orderNotes: model.OrderNotes);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintOrdersToPdf(stream, orders, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id, model.VendorId);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, "orders.pdf");
        }

        [HttpPost]
        public virtual IActionResult PdfInvoiceSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(_orderService.GetOrdersByIds(ids));
            }

            //a vendor should have access only to his products
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
            {
                orders = orders.Where(HasAccessToOrder).ToList();
                vendorId = _workContext.CurrentVendor.Id;
            }

            //ensure that we at least one order selected
            if (!orders.Any())
            {
                _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Orders.PdfInvoice.NoOrders"));
                return RedirectToAction("List");
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintOrdersToPdf(stream, orders, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id, vendorId);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, "orders.pdf");
        }

        //currently we use this method on the add product to order details pages
        [HttpPost]
        public virtual IActionResult ProductDetails_AttributeChange(int productId, bool validateAttributeConditions, IFormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                return new NullJsonResult();

            var errors = new List<string>();
            var attributeXml = ParseProductAttributes(product, form, errors);

            //conditional attributes
            var enabledAttributeMappingIds = new List<int>();
            var disabledAttributeMappingIds = new List<int>();
            if (validateAttributeConditions)
            {
                var attributes = _productAttributeService.GetProductAttributeMappingsByProductId(product.Id);
                foreach (var attribute in attributes)
                {
                    var conditionMet = _productAttributeParser.IsConditionMet(attribute, attributeXml);
                    if (!conditionMet.HasValue)
                        continue;

                    if (conditionMet.Value)
                        enabledAttributeMappingIds.Add(attribute.Id);
                    else
                        disabledAttributeMappingIds.Add(attribute.Id);
                }
            }

            return Json(new
            {
                enabledattributemappingids = enabledAttributeMappingIds.ToArray(),
                disabledattributemappingids = disabledAttributeMappingIds.ToArray(),
                message = errors.Any() ? errors.ToArray() : null
            });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveCC")]
        public virtual IActionResult EditCreditCardInfo(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            if (order.AllowStoringCreditCardNumber)
            {
                var cardType = model.CardType;
                var cardName = model.CardName;
                var cardNumber = model.CardNumber;
                var cardCvv2 = model.CardCvv2;
                var cardExpirationMonth = model.CardExpirationMonth;
                var cardExpirationYear = model.CardExpirationYear;

                order.CardType = _encryptionService.EncryptText(cardType);
                order.CardName = _encryptionService.EncryptText(cardName);
                order.CardNumber = _encryptionService.EncryptText(cardNumber);
                order.MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(cardNumber));
                order.CardCvv2 = _encryptionService.EncryptText(cardCvv2);
                order.CardExpirationMonth = _encryptionService.EncryptText(cardExpirationMonth);
                order.CardExpirationYear = _encryptionService.EncryptText(cardExpirationYear);
                _orderService.UpdateOrder(order);
            }

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Credit card info has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            //prepare model
            model = _orderModelFactory.PrepareOrderModel(model, order);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderTotals")]
        public virtual IActionResult EditOrderTotals(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            order.OrderSubtotalInclTax = model.OrderSubtotalInclTaxValue;
            order.OrderSubtotalExclTax = model.OrderSubtotalExclTaxValue;
            order.OrderSubTotalDiscountInclTax = model.OrderSubTotalDiscountInclTaxValue;
            order.OrderSubTotalDiscountExclTax = model.OrderSubTotalDiscountExclTaxValue;
            order.OrderShippingInclTax = model.OrderShippingInclTaxValue;
            order.OrderShippingExclTax = model.OrderShippingExclTaxValue;
            order.PaymentMethodAdditionalFeeInclTax = model.PaymentMethodAdditionalFeeInclTaxValue;
            order.PaymentMethodAdditionalFeeExclTax = model.PaymentMethodAdditionalFeeExclTaxValue;
            order.CheckoutAttributeDescription = model.CheckoutAttributeInfo;
            order.TaxRates = model.TaxRatesValue;
            order.OrderTax = model.TaxValue;
            order.OrderDiscount = model.OrderTotalDiscountValue;
            order.OrderTotal = model.OrderTotalValue;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Order totals have been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            //prepare model
            model = _orderModelFactory.PrepareOrderModel(model, order);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("save-shipping-method")]
        public virtual IActionResult EditShippingMethod(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            order.ShippingMethod = model.ShippingMethod;
            _orderService.UpdateOrder(order);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Shipping method has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            //prepare model
            model = _orderModelFactory.PrepareOrderModel(model, order);

            //selected panel
            SaveSelectedPanelName("order-billing-shipping", persistForTheNextRequest: false);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnSaveOrderItem")]
        public virtual IActionResult EditOrderItem(int id, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            //get order item identifier
            var orderItemId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnSaveOrderItem", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnSaveOrderItem".Length));

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId)
                ?? throw new ArgumentException("No order item found with the specified id");

            if (!decimal.TryParse(form["pvUnitPriceInclTax" + orderItemId], out var unitPriceInclTax))
                unitPriceInclTax = orderItem.UnitPriceInclTax;
            if (!decimal.TryParse(form["pvUnitPriceExclTax" + orderItemId], out var unitPriceExclTax))
                unitPriceExclTax = orderItem.UnitPriceExclTax;
            if (!int.TryParse(form["pvQuantity" + orderItemId], out var quantity))
                quantity = orderItem.Quantity;
            if (!decimal.TryParse(form["pvDiscountInclTax" + orderItemId], out var discountInclTax))
                discountInclTax = orderItem.DiscountAmountInclTax;
            if (!decimal.TryParse(form["pvDiscountExclTax" + orderItemId], out var discountExclTax))
                discountExclTax = orderItem.DiscountAmountExclTax;
            if (!decimal.TryParse(form["pvPriceInclTax" + orderItemId], out var priceInclTax))
                priceInclTax = orderItem.PriceInclTax;
            if (!decimal.TryParse(form["pvPriceExclTax" + orderItemId], out var priceExclTax))
                priceExclTax = orderItem.PriceExclTax;

            if (quantity > 0)
            {
                var qtyDifference = orderItem.Quantity - quantity;

                if (!_orderSettings.AutoUpdateOrderTotalsOnEditingOrder)
                {
                    orderItem.UnitPriceInclTax = unitPriceInclTax;
                    orderItem.UnitPriceExclTax = unitPriceExclTax;
                    orderItem.Quantity = quantity;
                    orderItem.DiscountAmountInclTax = discountInclTax;
                    orderItem.DiscountAmountExclTax = discountExclTax;
                    orderItem.PriceInclTax = priceInclTax;
                    orderItem.PriceExclTax = priceExclTax;
                    _orderService.UpdateOrder(order);
                }

                //adjust inventory
                _productService.AdjustInventory(orderItem.Product, qtyDifference, orderItem.AttributesXml,
                    string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.EditOrder"), order.Id));
            }
            else
            {
                //adjust inventory
                _productService.AdjustInventory(orderItem.Product, orderItem.Quantity, orderItem.AttributesXml,
                    string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.DeleteOrderItem"), order.Id));

                //delete item
                _orderService.DeleteOrderItem(orderItem);
            }

            //update order totals
            var updateOrderParameters = new UpdateOrderParameters
            {
                UpdatedOrder = order,
                UpdatedOrderItem = orderItem,
                PriceInclTax = unitPriceInclTax,
                PriceExclTax = unitPriceExclTax,
                DiscountAmountInclTax = discountInclTax,
                DiscountAmountExclTax = discountExclTax,
                SubTotalInclTax = priceInclTax,
                SubTotalExclTax = priceExclTax,
                Quantity = quantity
            };
            _orderProcessingService.UpdateOrderTotals(updateOrderParameters);

            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "Order item has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            //prepare model
            var model = _orderModelFactory.PrepareOrderModel(null, order);

            foreach (var warning in updateOrderParameters.Warnings)
                _notificationService.WarningNotification(warning);

            //selected panel
            SaveSelectedPanelName("order-products", persistForTheNextRequest: false);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnDeleteOrderItem")]
        public virtual IActionResult DeleteOrderItem(int id, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id });

            //get order item identifier
            var orderItemId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnDeleteOrderItem", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnDeleteOrderItem".Length));

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId)
                ?? throw new ArgumentException("No order item found with the specified id");

            if (_giftCardService.GetGiftCardsByPurchasedWithOrderItemId(orderItem.Id).Any())
            {
                //we cannot delete an order item with associated gift cards
                //a store owner should delete them first

                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Orders.OrderItem.DeleteAssociatedGiftCardRecordError"));

                //selected panel
                SaveSelectedPanelName("order-products", persistForTheNextRequest: false);

                return View(model);
            }
            else
            {
                //adjust inventory
                _productService.AdjustInventory(orderItem.Product, orderItem.Quantity, orderItem.AttributesXml,
                    string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.DeleteOrderItem"), order.Id));

                //delete item
                _orderService.DeleteOrderItem(orderItem);

                //update order totals
                var updateOrderParameters = new UpdateOrderParameters
                {
                    UpdatedOrder = order,
                    UpdatedOrderItem = orderItem
                };
                _orderProcessingService.UpdateOrderTotals(updateOrderParameters);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "Order item has been deleted",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                //prepare model
                var model = _orderModelFactory.PrepareOrderModel(null, order);

                foreach (var warning in updateOrderParameters.Warnings)
                    _notificationService.WarningNotification(warning);

                //selected panel
                SaveSelectedPanelName("order-products", persistForTheNextRequest: false);

                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnResetDownloadCount")]
        public virtual IActionResult ResetDownloadCount(int id, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //get order item identifier
            var orderItemId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnResetDownloadCount", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnResetDownloadCount".Length));

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId)
                ?? throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            orderItem.DownloadCount = 0;
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            //prepare model
            var model = _orderModelFactory.PrepareOrderModel(null, order);

            //selected panel
            SaveSelectedPanelName("order-products", persistForTheNextRequest: false);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnPvActivateDownload")]
        public virtual IActionResult ActivateDownloadItem(int id, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //get order item identifier
            var orderItemId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnPvActivateDownload", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnPvActivateDownload".Length));

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId)
                ?? throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            orderItem.IsDownloadActivated = !orderItem.IsDownloadActivated;
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            //prepare model
            var model = _orderModelFactory.PrepareOrderModel(null, order);

            //selected panel
            SaveSelectedPanelName("order-products", persistForTheNextRequest: false);
            return View(model);
        }

        public virtual IActionResult UploadLicenseFilePopup(int id, int orderItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            //try to get an order item with the specified id
            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == orderItemId)
                ?? throw new ArgumentException("No order item found with the specified id");

            if (!orderItem.Product.IsDownload)
                throw new ArgumentException("Product is not downloadable");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            //prepare model
            var model = _orderModelFactory.PrepareUploadLicenseModel(new UploadLicenseModel(), order, orderItem);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("uploadlicense")]
        public virtual IActionResult UploadLicenseFilePopup(UploadLicenseModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                return RedirectToAction("List");

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == model.OrderItemId)
                ?? throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            //attach license
            if (model.LicenseDownloadId > 0)
                orderItem.LicenseDownloadId = model.LicenseDownloadId;
            else
                orderItem.LicenseDownloadId = null;
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            //success
            ViewBag.RefreshPage = true;

            return View(model);
        }

        [HttpPost, ActionName("UploadLicenseFilePopup")]
        [FormValueRequired("deletelicense")]
        public virtual IActionResult DeleteLicenseFilePopup(UploadLicenseModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                return RedirectToAction("List");

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Id == model.OrderItemId)
                ?? throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (_workContext.CurrentVendor != null && !HasAccessToOrderItem(orderItem))
                return RedirectToAction("List");

            //attach license
            orderItem.LicenseDownloadId = null;
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            //success
            ViewBag.RefreshPage = true;

            return View(model);
        }

        public virtual IActionResult AddProductToOrder(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            //prepare model
            var model = _orderModelFactory.PrepareAddProductToOrderSearchModel(new AddProductToOrderSearchModel(), order);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddProductToOrder(AddProductToOrderSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(searchModel.OrderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Content(string.Empty);

            //prepare model
            var model = _orderModelFactory.PrepareAddProductToOrderListModel(searchModel, order);

            return Json(model);
        }

        public virtual IActionResult AddProductToOrderDetails(int orderId, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(orderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //try to get a product with the specified id
            var product = _productService.GetProductById(productId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            //prepare model
            var model = _orderModelFactory.PrepareAddProductToOrderModel(new AddProductToOrderModel(), order, product);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddProductToOrderDetails(int orderId, int productId, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(orderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //try to get a product with the specified id
            var product = _productService.GetProductById(productId)
                ?? throw new ArgumentException("No product found with the specified id");

            //basic properties
            decimal.TryParse(form["UnitPriceInclTax"], out var unitPriceInclTax);
            decimal.TryParse(form["UnitPriceExclTax"], out var unitPriceExclTax);
            int.TryParse(form["Quantity"], out var quantity);
            decimal.TryParse(form["SubTotalInclTax"], out var priceInclTax);
            decimal.TryParse(form["SubTotalExclTax"], out var priceExclTax);

            //warnings
            var warnings = new List<string>();

            //attributes
            var attributesXml = ParseProductAttributes(product, form, warnings);

            //gift cards
            attributesXml = AddGiftCards(form, product, attributesXml, out var recipientName, out var recipientEmail, out var senderName, out var senderEmail, out var giftCardMessage);

            //rental product
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.IsRental)
            {
                ParseRentalDates(form, out rentalStartDate, out rentalEndDate);
            }

            //warnings
            warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(order.Customer, ShoppingCartType.ShoppingCart, product, quantity, attributesXml));
            warnings.AddRange(_shoppingCartService.GetShoppingCartItemGiftCardWarnings(ShoppingCartType.ShoppingCart, product, attributesXml));
            warnings.AddRange(_shoppingCartService.GetRentalProductWarnings(product, rentalStartDate, rentalEndDate));
            if (!warnings.Any())
            {
                //no errors

                //attributes
                var attributeDescription = _productAttributeFormatter.FormatAttributes(product, attributesXml, order.Customer);

                //weight
                var itemWeight = _shippingService.GetShoppingCartItemWeight(product, attributesXml);

                //save item
                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    Order = order,
                    ProductId = product.Id,
                    UnitPriceInclTax = unitPriceInclTax,
                    UnitPriceExclTax = unitPriceExclTax,
                    PriceInclTax = priceInclTax,
                    PriceExclTax = priceExclTax,
                    OriginalProductCost = _priceCalculationService.GetProductCost(product, attributesXml),
                    AttributeDescription = attributeDescription,
                    AttributesXml = attributesXml,
                    Quantity = quantity,
                    DiscountAmountInclTax = decimal.Zero,
                    DiscountAmountExclTax = decimal.Zero,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0,
                    ItemWeight = itemWeight,
                    RentalStartDateUtc = rentalStartDate,
                    RentalEndDateUtc = rentalEndDate
                };
                order.OrderItems.Add(orderItem);
                _orderService.UpdateOrder(order);

                //adjust inventory
                _productService.AdjustInventory(orderItem.Product, -orderItem.Quantity, orderItem.AttributesXml,
                    string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.EditOrder"), order.Id));

                //update order totals
                var updateOrderParameters = new UpdateOrderParameters
                {
                    UpdatedOrder = order,
                    UpdatedOrderItem = orderItem,
                    PriceInclTax = unitPriceInclTax,
                    PriceExclTax = unitPriceExclTax,
                    SubTotalInclTax = priceInclTax,
                    SubTotalExclTax = priceExclTax,
                    Quantity = quantity
                };
                _orderProcessingService.UpdateOrderTotals(updateOrderParameters);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "A new order item has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                //gift cards
                if (product.IsGiftCard)
                {
                    for (var i = 0; i < orderItem.Quantity; i++)
                    {
                        var gc = new GiftCard
                        {
                            GiftCardType = product.GiftCardType,
                            PurchasedWithOrderItem = orderItem,
                            Amount = unitPriceExclTax,
                            IsGiftCardActivated = false,
                            GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                            RecipientName = recipientName,
                            RecipientEmail = recipientEmail,
                            SenderName = senderName,
                            SenderEmail = senderEmail,
                            Message = giftCardMessage,
                            IsRecipientNotified = false,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        _giftCardService.InsertGiftCard(gc);
                    }
                }

                //redirect to order details page
                foreach (var warning in updateOrderParameters.Warnings)
                    _notificationService.WarningNotification(warning);

                //selected panel
                SaveSelectedPanelName("order-products");
                return RedirectToAction("Edit", "Order", new { id = order.Id });
            }

            //prepare model
            var model = _orderModelFactory.PrepareAddProductToOrderModel(new AddProductToOrderModel(), order, product);
            model.Warnings.AddRange(warnings);

            return View(model);
        }

        #endregion

        #endregion

        #region Addresses

        public virtual IActionResult AddressEdit(int addressId, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            //try to get an address with the specified id
            var address = _addressService.GetAddressById(addressId)
                ?? throw new ArgumentException("No address found with the specified id", nameof(addressId));

            //prepare model
            var model = _orderModelFactory.PrepareOrderAddressModel(new OrderAddressModel(), order, address);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult AddressEdit(OrderAddressModel model, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = order.Id });

            //try to get an address with the specified id
            var address = _addressService.GetAddressById(model.Address.Id)
                ?? throw new ArgumentException("No address found with the specified id");

            //custom address attributes
            var customAttributes = _addressAttributeParser.ParseCustomAddressAttributes(form);
            var customAttributeWarnings = _addressAttributeParser.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                address.CustomAttributes = customAttributes;
                _addressService.UpdateAddress(address);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "Address has been edited",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, orderId = model.OrderId });
            }

            //prepare model
            model = _orderModelFactory.PrepareOrderAddressModel(model, order, address);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Shipments

        public virtual IActionResult ShipmentList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductBulkUpdate) && !_permissionService.Authorize(StandardPermissionProvider.ManageOrderShipment))
                return AccessDeniedView();

            //prepare model
            var model = _orderModelFactory.PrepareShipmentSearchModel(new ShipmentSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult ShipmentListSelect(ShipmentSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders) && !_permissionService.Authorize(StandardPermissionProvider.ManageOrderShipment))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _orderModelFactory.PrepareShipmentListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult ShipmentsByOrder(OrderShipmentSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders) && !_permissionService.Authorize(StandardPermissionProvider.ManageOrderShipment))
                return AccessDeniedDataTablesJson();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(searchModel.OrderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToOrder(order))
                return Content(string.Empty);

            //prepare model
            var model = _orderModelFactory.PrepareOrderShipmentListModel(searchModel, order);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult ShipmentsItemsByShipmentId(ShipmentItemSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            //try to get a shipment with the specified id
            var shipment = _shipmentService.GetShipmentById(searchModel.ShipmentId)
                ?? throw new ArgumentException("No shipment found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return Content(string.Empty);

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(shipment.OrderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToOrder(order))
                return Content(string.Empty);

            //prepare model
            searchModel.SetGridPageSize();
            var model = _orderModelFactory.PrepareShipmentItemListModel(searchModel, shipment);

            return Json(model);
        }

        public virtual IActionResult AddShipment(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToOrder(order))
                return RedirectToAction("List");

            //prepare model
            var model = _orderModelFactory.PrepareShipmentModel(new ShipmentModel(), null, order);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult AddShipment(int orderId, IFormCollection form, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToOrder(order))
                return RedirectToAction("List");

            var orderItems = order.OrderItems;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                orderItems = orderItems.Where(HasAccessToOrderItem).ToList();
            }

            Shipment shipment = null;
            decimal? totalWeight = null;
            foreach (var orderItem in orderItems)
            {
                //is shippable
                if (!orderItem.Product.IsShipEnabled)
                    continue;

                //ensure that this product can be shipped (have at least one item to ship)
                var maxQtyToAdd = _orderService.GetTotalNumberOfItemsCanBeAddedToShipment(orderItem);
                if (maxQtyToAdd <= 0)
                    continue;

                var qtyToAdd = 0; //parse quantity
                foreach (var formKey in form.Keys)
                    if (formKey.Equals($"qtyToAdd{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                var warehouseId = 0;
                if (orderItem.Product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    orderItem.Product.UseMultipleWarehouses)
                {
                    //multiple warehouses supported
                    //warehouse is chosen by a store owner
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"warehouse_{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(form[formKey], out warehouseId);
                            break;
                        }
                }
                else
                {
                    //multiple warehouses are not supported
                    warehouseId = orderItem.Product.WarehouseId;
                }

                foreach (var formKey in form.Keys)
                    if (formKey.Equals($"qtyToAdd{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                //validate quantity
                if (qtyToAdd <= 0)
                    continue;
                if (qtyToAdd > maxQtyToAdd)
                    qtyToAdd = maxQtyToAdd;

                //ok. we have at least one item. let's create a shipment (if it does not exist)

                var orderItemTotalWeight = orderItem.ItemWeight * qtyToAdd;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }

                if (shipment == null)
                {
                    var trackingNumber = form["TrackingNumber"];
                    var adminComment = form["AdminComment"];
                    shipment = new Shipment
                    {
                        OrderId = order.Id,
                        TrackingNumber = trackingNumber,
                        TotalWeight = null,
                        ShippedDateUtc = null,
                        DeliveryDateUtc = null,
                        AdminComment = adminComment,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                }

                //create a shipment item
                var shipmentItem = new ShipmentItem
                {
                    OrderItemId = orderItem.Id,
                    Quantity = qtyToAdd,
                    WarehouseId = warehouseId
                };
                shipment.ShipmentItems.Add(shipmentItem);
            }

            //if we have at least one item in the shipment, then save it
            if (shipment != null && shipment.ShipmentItems.Any())
            {
                shipment.TotalWeight = totalWeight;
                _shipmentService.InsertShipment(shipment);

                //add a note
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "A shipment has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
                LogEditOrder(order.Id);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Orders.Shipments.Added"));
                return continueEditing
                           ? RedirectToAction("ShipmentDetails", new { id = shipment.Id })
                           : RedirectToAction("Edit", new { id = orderId });
            }

            _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.NoProductsSelected"));

            return RedirectToAction("AddShipment", new { orderId });
        }

        public virtual IActionResult ShipmentDetails(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            //prepare model
            var model = _orderModelFactory.PrepareShipmentModel(null, shipment, null);

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult DeleteShipment(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                if (orderItem == null)
                    continue;

                _productService.ReverseBookedInventory(orderItem.Product, shipmentItem,
                    string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.DeleteShipment"), shipment.OrderId));
            }

            var orderId = shipment.OrderId;
            _shipmentService.DeleteShipment(shipment);

            var order = _orderService.GetOrderById(orderId);
            //add a note
            order.OrderNotes.Add(new OrderNote
            {
                Note = "A shipment has been deleted",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            _orderService.UpdateOrder(order);
            LogEditOrder(order.Id);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Orders.Shipments.Deleted"));
            return RedirectToAction("Edit", new { id = orderId });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("settrackingnumber")]
        public virtual IActionResult SetTrackingNumber(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            shipment.TrackingNumber = model.TrackingNumber;
            _shipmentService.UpdateShipment(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setadmincomment")]
        public virtual IActionResult SetShipmentAdminComment(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            shipment.AdminComment = model.AdminComment;
            _shipmentService.UpdateShipment(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasshipped")]
        public virtual IActionResult SetAsShipped(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.Ship(shipment, true);
                LogEditOrder(shipment.OrderId);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("saveshippeddate")]
        public virtual IActionResult EditShippedDate(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            try
            {
                if (!model.ShippedDateUtc.HasValue)
                {
                    throw new Exception("Enter shipped date");
                }

                shipment.ShippedDateUtc = model.ShippedDateUtc;
                _shipmentService.UpdateShipment(shipment);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasdelivered")]
        public virtual IActionResult SetAsDelivered(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.Deliver(shipment, true);
                LogEditOrder(shipment.OrderId);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("savedeliverydate")]
        public virtual IActionResult EditDeliveryDate(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            try
            {
                if (!model.DeliveryDateUtc.HasValue)
                {
                    throw new Exception("Enter delivery date");
                }

                shipment.DeliveryDateUtc = model.DeliveryDateUtc;
                _shipmentService.UpdateShipment(shipment);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                _notificationService.ErrorNotification(exc);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        public virtual IActionResult PdfPackagingSlip(int shipmentId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && !HasAccessToShipment(shipment))
                return RedirectToAction("List");

            var shipments = new List<Shipment>
            {
                shipment
            };

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintPackagingSlipsToPdf(stream, shipments, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, $"packagingslip_{shipment.Id}.pdf");
        }

        [HttpPost, ActionName("ShipmentList")]
        [FormValueRequired("exportpackagingslips-all")]
        public virtual IActionResult PdfPackagingSlipAll(ShipmentSearchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders) && !_permissionService.Authorize(StandardPermissionProvider.ManageOrderShipment))
                return AccessDeniedView();

            var startDateValue = model.StartDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            var endDateValue = model.EndDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            //a vendor should have access only to his products
            var vendorId = 0;
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            //load shipments
            var shipments = _shipmentService.GetAllShipments(vendorId: vendorId,
                warehouseId: model.WarehouseId,
                shippingCountryId: model.CountryId,
                shippingStateId: model.StateProvinceId,
                shippingCounty: model.County,
                shippingCity: model.City,
                trackingNumber: model.TrackingNumber,
                loadNotShipped: model.LoadNotShipped,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue);

            //ensure that we at least one shipment selected
            if (!shipments.Any())
            {
                _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.NoShipmentsSelected"));
                return RedirectToAction("ShipmentList");
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintPackagingSlipsToPdf(stream, shipments, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, "packagingslips.pdf");
        }

        [HttpPost]
        public virtual IActionResult PdfPackagingSlipSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                shipments.AddRange(_shipmentService.GetShipmentsByIds(ids));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                shipments = shipments.Where(HasAccessToShipment).ToList();
            }

            //ensure that we at least one shipment selected
            if (!shipments.Any())
            {
                _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Orders.Shipments.NoShipmentsSelected"));
                return RedirectToAction("ShipmentList");
            }

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                _pdfService.PrintPackagingSlipsToPdf(stream, shipments, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : _workContext.WorkingLanguage.Id);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, "packagingslips.pdf");
        }

        [HttpPost]
        public virtual IActionResult SetAsShippedSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                shipments.AddRange(_shipmentService.GetShipmentsByIds(selectedIds.ToArray()));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                shipments = shipments.Where(HasAccessToShipment).ToList();
            }

            foreach (var shipment in shipments)
            {
                try
                {
                    _orderProcessingService.Ship(shipment, true);
                }
                catch
                {
                    //ignore any exception
                }
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual IActionResult SetAsDeliveredSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                shipments.AddRange(_shipmentService.GetShipmentsByIds(selectedIds.ToArray()));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                shipments = shipments.Where(HasAccessToShipment).ToList();
            }

            foreach (var shipment in shipments)
            {
                try
                {
                    _orderProcessingService.Deliver(shipment, true);
                }
                catch
                {
                    //ignore any exception
                }
            }

            return Json(new { Result = true });
        }

        #endregion

        #region Order notes

        [HttpPost]
        public virtual IActionResult OrderNotesSelect(OrderNoteSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(searchModel.OrderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return Content(string.Empty);

            //prepare model
            var model = _orderModelFactory.PrepareOrderNoteListModel(searchModel, order);

            return Json(model);
        }

        public virtual IActionResult OrderNoteAdd(int orderId, int downloadId, bool displayToCustomer, string message)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(message))
                return ErrorJson(_localizationService.GetResource("Admin.Orders.OrderNotes.Fields.Note.Validation"));

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return ErrorJson("Order cannot be loaded");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return ErrorJson("No access for vendors");

            var orderNote = new OrderNote
            {
                DisplayToCustomer = displayToCustomer,
                Note = message,
                DownloadId = downloadId,
                CreatedOnUtc = DateTime.UtcNow
            };

            order.OrderNotes.Add(orderNote);
            _orderService.UpdateOrder(order);

            //new order notification
            if (displayToCustomer)
            {
                //email
                _workflowMessageService.SendNewOrderNoteAddedCustomerNotification(orderNote, _workContext.WorkingLanguage.Id);
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual IActionResult OrderNoteDelete(int id, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = _orderService.GetOrderById(orderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //a vendor does not have access to this functionality
            if (_workContext.CurrentVendor != null)
                return RedirectToAction("Edit", "Order", new { id = orderId });

            //try to get an order note with the specified id
            var orderNote = order.OrderNotes.FirstOrDefault(on => on.Id == id)
                ?? throw new ArgumentException("No order note found with the specified id");

            _orderService.DeleteOrderNote(orderNote);

            return new NullJsonResult();
        }

        #endregion

        #region Reports

        [HttpPost]
        public virtual IActionResult BestsellersBriefReportByQuantityList(BestsellerBriefSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            //if (_workContext.CurrentCustomer.CustomerRoles.Any(r => r.SystemName == Core.Domain.Customers.NopCustomerDefaults.SalemanRoleName))
            //    searchModel.SalesmanId = _workContext.CurrentCustomer.Id;

            //prepare model
            var model = _orderModelFactory.PrepareBestsellerBriefListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult BestsellersBriefReportByAmountList(BestsellerBriefSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            //if (_workContext.CurrentCustomer.CustomerRoles.Any(r => r.SystemName == Core.Domain.Customers.NopCustomerDefaults.SalemanRoleName))
            //    searchModel.SalesmanId = _workContext.CurrentCustomer.Id;

            //prepare model
            var model = _orderModelFactory.PrepareBestsellerBriefListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult OrderAverageReportList(OrderAverageReportSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            if (_workContext.CurrentCustomer.CustomerRoles.Any(r => r.SystemName == Core.Domain.Customers.NopCustomerDefaults.SalemanRoleName))
                searchModel.SalesmanId = _workContext.CurrentCustomer.Id;

            //a vendor doesn't have access to this report
            if (_workContext.CurrentVendor != null)
                return Content(string.Empty);

            //prepare model
            var model = _orderModelFactory.PrepareOrderAverageReportListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult OrderIncompleteReportList(OrderIncompleteReportSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            if (_workContext.CurrentCustomer.CustomerRoles.Any(r => r.SystemName == Core.Domain.Customers.NopCustomerDefaults.SalemanRoleName))
                searchModel.SalesmanId = _workContext.CurrentCustomer.Id;

            //a vendor doesn't have access to this report
            if (_workContext.CurrentVendor != null)
                return Content(string.Empty);

            //prepare model
            var model = _orderModelFactory.PrepareOrderIncompleteReportListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult LoadOrderStatistics(string period)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return Content(string.Empty);

            int salesmanId = 0;
            if (_workContext.CurrentCustomer.CustomerRoles.Any(r => r.SystemName == Core.Domain.Customers.NopCustomerDefaults.SalemanRoleName))
                salesmanId = _workContext.CurrentCustomer.Id;

            //a vendor doesn't have access to this report
            if (_workContext.CurrentVendor != null)
                return Content(string.Empty);

            var result = new List<object>();

            var nowDt = _dateTimeHelper.ConvertToUserTime(DateTime.Now);
            var timeZone = _dateTimeHelper.CurrentTimeZone;

            var culture = new CultureInfo(_workContext.WorkingLanguage.LanguageCulture);

            switch (period)
            {
                case "year":
                    //year statistics
                    var yearAgoDt = nowDt.AddYears(-1).AddMonths(1);
                    var searchYearDateUser = new DateTime(yearAgoDt.Year, yearAgoDt.Month, 1);
                    for (var i = 0; i <= 12; i++)
                    {
                        result.Add(new
                        {
                            date = searchYearDateUser.Date.ToString("Y", culture),
                            value = _orderService.SearchOrders2(
                                createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser, timeZone),
                                createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchYearDateUser.AddMonths(1), timeZone),
                                salemanId: salesmanId,
                                pageIndex: 0,
                                pageSize: 1, getOnlyTotalCount: true).TotalCount.ToString()
                        });

                        searchYearDateUser = searchYearDateUser.AddMonths(1);
                    }

                    break;
                case "month":
                    //month statistics
                    var monthAgoDt = nowDt.AddDays(-30);
                    var searchMonthDateUser = new DateTime(monthAgoDt.Year, monthAgoDt.Month, monthAgoDt.Day);
                    for (var i = 0; i <= 30; i++)
                    {
                        result.Add(new
                        {
                            date = searchMonthDateUser.Date.ToString("M", culture),
                            value = _orderService.SearchOrders2(
                                createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser, timeZone),
                                createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchMonthDateUser.AddDays(1), timeZone),
                                salemanId: salesmanId,
                                pageIndex: 0,
                                pageSize: 1, getOnlyTotalCount: true).TotalCount.ToString()
                        });

                        searchMonthDateUser = searchMonthDateUser.AddDays(1);
                    }

                    break;
                case "week":
                default:
                    //week statistics
                    var weekAgoDt = nowDt.AddDays(-7);
                    var searchWeekDateUser = new DateTime(weekAgoDt.Year, weekAgoDt.Month, weekAgoDt.Day);
                    for (var i = 0; i <= 7; i++)
                    {
                        result.Add(new
                        {
                            date = searchWeekDateUser.Date.ToString("d dddd", culture),
                            value = _orderService.SearchOrders2(
                                createdFromUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser, timeZone),
                                createdToUtc: _dateTimeHelper.ConvertToUtcTime(searchWeekDateUser.AddDays(1), timeZone),
                                salemanId: salesmanId,
                                pageIndex: 0,
                                pageSize: 1, getOnlyTotalCount: true).TotalCount.ToString()
                        });

                        searchWeekDateUser = searchWeekDateUser.AddDays(1);
                    }

                    break;
            }

            return Json(result);
        }

        #endregion
    }
}