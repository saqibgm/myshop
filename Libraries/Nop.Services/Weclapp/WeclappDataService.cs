using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Weclapp.model;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Weclapp
{
    public class WeclappDataService : IWeclappDataService
    {
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
        //private readonly ITierPriceService _tierPriceService;

        //public CustomProductService(IProductService productService, ITierPriceService tierPriceService)
        //{
        //    _productService = productService;
        //    _tierPriceService = tierPriceService;
        //}
        public WeclappDataService(
        IProductService productService,
        ICustomerService customerService,
        IOrderService orderService,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        ICategoryService categoryService,
        IPictureService pictureService,
        IUrlRecordService urlRecordService,
        IStoreMappingService storeMappingService,
        IGenericAttributeService genericAttributeService)
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
        }
        public IList<Core.Domain.Catalog.Product> GetNopProducts()
        {
            var  productList= _productService.GetProductsByIds(new int[] { 5511, 5510, 5509, 5508, 5507, 5506 });

            return productList;
        }

        public List<NopCustomer> GetNopCustomers()
        {
            var customers = _customerService.GetCustomersByIds(new int[] { 21286953, 21229763, 21227714, 21210569, 21180522});
            //21286953, 21229763, 21227714, 21210569, 21180522, 21157465, 21136588, 21126354, 21047703, 21021435, 21004579, 20845758, 20751568, 20490355, 20456725
            var customerList = new List<NopCustomer>();

            foreach (var customer in customers)
            {
                if (customer.ShippingAddress != null && !string.IsNullOrEmpty(customer.ShippingAddress.Company))
                {
                    var vatNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.VatNumberAttribute);

                    var customerData = new NopCustomer();
                    customerData.customerNumber = customer.CustomerGuid.ToString();
                    customerData.nopid = customer.Id;
                    customerData.id = "";
                    customerData.number = customer.CustomerGuid.ToString();
                    customerData.name = $"{customer.Username}";
                    customerData.email = customer.Email;
                    customerData.vatRegistrationNumber = vatNumber;
                    customerData.partyType = "ORGANIZATION";
                    customerData.customerType = "CUSTOMER";
                    customerData.blocked = false;
                    customerData.deliveryBlock = false;
                    customerData.insolvent = false;
                    customerData.insured = false;
                    customerData.invoiceBlock = false;
                    customerData.optIn = false;
                    customerData.optInLetter = false;
                    customerData.optInPhone = false;
                    customerData.optInSms = false;
                    customerData.responsibleUserFixed = false;
                    customerData.useCustomsTariffNumber = false;
                    customerData.company = customer.ShippingAddress == null ? "" : customer.ShippingAddress.Company;
                    customerData.addresses = new List<NopAddress>
                       {
                        new NopAddress
                        {
                            id="",
                            street1 = customer.BillingAddress?.Address1 ?? "N/A",
                            zipCode = customer.BillingAddress?.ZipPostalCode ?? "00000",
                            city = customer.BillingAddress?.City ?? "N/A",
                            country = customer.BillingAddress?.Country.Name,
                            countryCode = customer.BillingAddress?.Country?.TwoLetterIsoCode ?? "DE",
                            primeAddress=true
                        }
                    };
                    customerList.Add(customerData);
                }
            }

            return customerList;
        }

        public List<NopOrder> GetNopOrders()
        {
            var orderList = new List<NopOrder>();
            var orders = _orderService.GetOrdersByIds(new int[] { 378441, 378440, 378439, 378438, 378437 });
            //378446,378445,378444,378443,378442,378441,378440,378439,378438,378437

            foreach (var order in orders)
            {
                if (order.ShippingAddress != null && !string.IsNullOrEmpty(order.ShippingAddress.Company))
                {
                    var orderData = new NopOrder();
                    orderData.customerOrderNumber = order.OrderGuid.ToString();
                    orderData.nopid = order.Id;
                    orderData.id = "";
                    orderData.customerNumber = order.CustomerId.ToString();
                    orderData.customerId = order.CustomerId.ToString();
                    orderData.currency = order.CustomerCurrencyCode;
                    orderData.orderDate = order.CreatedOnUtc.ToString();
                    orderData.status = order.OrderStatus.ToString();
                    orderData.orderItems = new List<NopOrderItem>();
                    foreach (var item in order.OrderItems)
                    {
                        var nItem = new NopOrderItem
                        {
                            id = "",
                            articleNumber = item.Product.Sku ?? item.Product.Id.ToString(),
                            quantity = item.Quantity.ToString(),
                            price = item.PriceInclTax.ToString(),
                            //taxRate = order.TaxRates.ToString()
                        };
                        orderData.orderItems.Add(nItem);
                    }
                    orderList.Add(orderData);
                }
            }

            return orderList;
        }
    }
}

