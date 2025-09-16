using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Services.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;


namespace Nop.Services.Orders
{
    /// <summary>
    /// Order report service
    /// </summary>
    public partial class OrderReportService : IOrderReportService
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public OrderReportService(CatalogSettings catalogSettings,
            IDateTimeHelper dateTimeHelper,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Product> productRepository,
            IRepository<Customer> customerRepository,
            IWorkContext workContext,
            ICustomerService customerService,
            IRepository<StoreMapping> storeMappingRepository)
        {
            _catalogSettings = catalogSettings;
            _dateTimeHelper = dateTimeHelper;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _workContext = workContext;
            _storeMappingRepository = storeMappingRepository;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get "order by country" report
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="os">Order status</param>
        /// <param name="ps">Payment status</param>
        /// <param name="ss">Shipping status</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <returns>Result</returns>
        public virtual IList<OrderByCountryReportLine> GetCountryReport(int storeId, OrderStatus? os,
            PaymentStatus? ps, ShippingStatus? ss, DateTime? startTimeUtc, DateTime? endTimeUtc)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;

            var query = _orderRepository.Table;
            query = query.Where(o => !o.Deleted);
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (orderStatusId.HasValue)
                query = query.Where(o => o.OrderStatusId == orderStatusId.Value);
            if (paymentStatusId.HasValue)
                query = query.Where(o => o.PaymentStatusId == paymentStatusId.Value);
            if (shippingStatusId.HasValue)
                query = query.Where(o => o.ShippingStatusId == shippingStatusId.Value);
            if (startTimeUtc.HasValue)
                query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
            if (endTimeUtc.HasValue)
                query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);

            var report = (from oq in query
                          group oq by oq.BillingAddress.CountryId
                          into result
                          select new
                          {
                              CountryId = result.Key,
                              TotalOrders = result.Count(),
                              SumOrders = result.Sum(o => o.OrderTotal)
                          })
                .OrderByDescending(x => x.SumOrders)
                .Select(r => new OrderByCountryReportLine
                {
                    CountryId = r.CountryId,
                    TotalOrders = r.TotalOrders,
                    SumOrders = r.SumOrders
                }).ToList();

            return report;
        }
        /// <summary>
        /// Get "order by country" report
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="os">Order status</param>
        /// <param name="ps">Payment status</param>
        /// <param name="ss">Shipping status</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <returns>Result</returns>
        public virtual IList<OrderByCommissionModelReportLine> GetCommissionReport(int storeId, int? salemanId, OrderStatus? os,
            PaymentStatus? ps, DateTime? startTimeUtc, DateTime? endTimeUtc)
        {

            var report = new List<OrderByCommissionModelReportLine>();
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            //int? shippingStatusId = null;
            //if (ss.HasValue)
            //    shippingStatusId = (int)ss.Value;

            var query = _orderRepository.Table.Where(q => q.OrderStatusId == (int)OrderStatus.Complete && !q.Deleted);
            var CustomerRepo = _customerRepository.Table;//.Where(c => salemanId == null || salemanId == 0 || c.Id == salemanId);
            query = query.Where(o => !o.Deleted);
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (orderStatusId.HasValue)
                query = query.Where(o => o.OrderStatusId == orderStatusId.Value);
            if (paymentStatusId.HasValue)
                query = query.Where(o => o.PaymentStatusId == paymentStatusId.Value);
            //if (shippingStatusId.HasValue)
            //    query = query.Where(o => o.ShippingStatusId == shippingStatusId.Value);
            if (startTimeUtc.HasValue)
                query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
            if (endTimeUtc.HasValue)
                query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);
            if (salemanId != null && salemanId != 0)
                query = query.Where(o => o.SalemanId >= salemanId);
            //   //******************************  CANDIATE ORDER WRT FILTERS ***********************************************//

            var QryResult = (from oq in query
                             join c in CustomerRepo on oq.CustomerId equals c.Id
                             where c.TeamMemberOfSaleman > 0
                             //where salemanId == null || salemanId == 0 || c.Id == salemanId
                             select new
                             {
                                 SalemanId = oq.Customer.TeamMemberOfSaleman,
                                 OrderGuid = oq.OrderGuid,
                                 OrderTotal = oq.OrderTotal,
                                 ProfitTotal = oq.Profit

                             });

            var FirstRes = QryResult.ToList();
            if (FirstRes.Count > 0)
            {
                //******************************  Saleman Bio Data ***********************************************//
                var SQuery = (from fr in FirstRes
                              join c in CustomerRepo on fr.SalemanId equals c.Id
                              where fr.SalemanId > 0 && (salemanId == null || salemanId == 0 || c.Id == salemanId)
                              select new
                              {
                                  fr.SalemanId,
                                  fr.OrderTotal,
                                  fr.ProfitTotal,
                                  c.CommissionMinTargert,
                                  c.CommissionPercentage,
                                  BasicSalary = c.BasicSalary
                              }).ToList();

                //******************************   PREPARE REPORT DATA ***********************************************//
                var report1 = (from oq in SQuery
                               group oq by new { oq.SalemanId, oq.BasicSalary, oq.CommissionPercentage, oq.CommissionMinTargert }
                      into Commission
                               select new
                               {
                                   SalemanId = Commission.Key.SalemanId,
                                   TotalSale = Commission.Sum(o => o.OrderTotal),
                                   CommissionPercentage = Commission.Key.CommissionPercentage,
                                   BasicSalary = Commission.Key.BasicSalary,
                                   CommissionMinTargert = Commission.Key.CommissionMinTargert,
                                   TotalOrders = Commission.Count(),
                                   ProfitTotal = Commission.Sum(o => o.ProfitTotal)

                               });
                report = report1
      .OrderByDescending(x => x.TotalOrders)
      .Select(r => new OrderByCommissionModelReportLine
      {
          SalemanId = r.SalemanId,
          TotalSale = r.TotalSale,
          TotalOrders = r.TotalOrders,
          TotalProfit = r.ProfitTotal.Value,
          CommissionAmount = (r.ProfitTotal > r.CommissionMinTargert) ? Math.Round(((r.ProfitTotal.Value * r.CommissionPercentage.Value / 100)), 2) : r.BasicSalary ?? 0,
          TotalSalemanSalaryIncCommission = (r.ProfitTotal > r.CommissionMinTargert) ? Math.Round(((r.ProfitTotal.Value * r.CommissionPercentage.Value / 100)), 2) : r.BasicSalary ?? 0,
          BasicSalary = Convert.ToDecimal(r.BasicSalary),
          CommissionPercentage= r.CommissionPercentage.Value

      }).ToList();
            }

            return report;
        }

        /// <summary>
        /// Get "order by country" report
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="os">Order status</param>
        /// <param name="ps">Payment status</param>
        /// <param name="ss">Shipping status</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <returns>Result</returns>
        public virtual IList<OrderBySalemanStatisticsReportLine> GetSalemanStatisticsReport(int storeId, int SalemanId, DateTime startTimeUtc, DateTime endTimeUtc)
        {

            var report = new List<OrderBySalemanStatisticsReportLine>();
            // ONly need to consider Paid only            
            //endTimeUtc = new DateTime(endTimeUtc.Year, endTimeUtc.Month, 1);
            //******************************  For Commission Defaults***********************************************//
            var pdIds = new List<int>();
            pdIds.Add((int)Nop.Core.Domain.Payments.PaymentStatus.Paid);

            var query = _orderRepository.Table;
            var CustomerRepo = _customerRepository.Table;
            var queryItems = _orderItemRepository.Table;
            query = query.Where(o => !o.Deleted);
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (SalemanId > 0)
                query = query.Where(o => o.Customer.TeamMemberOfSaleman == SalemanId);
            if (true)
                query = query.Where(o => o.PaymentStatusId == (int)Nop.Core.Domain.Payments.PaymentStatus.Paid);
            // Dates are manadatory, bc auto assigned
            query = query.Where(o => o.CreatedOnUtc >= startTimeUtc);
            query = query.Where(o => o.CreatedOnUtc <= endTimeUtc);
            //******************************  For Profit Calculation***********************************************//
            var OrderList = (from oq in query
                             join c in CustomerRepo on oq.CustomerId equals c.Id
                             where oq.CustomerId > 0
                             select oq
                             ).ToList();

            //******************************  Customers Get All Respective Order Items***********************************************//
            var PItemList = (from orderItem in queryItems
                             join o in OrderList on orderItem.OrderId equals o.Id
                             select orderItem).ToList();
            //******************************  Saleman Bio Data ***********************************************//
            var refinedQry = (from oq in query
                              join c in CustomerRepo on Convert.ToInt32(oq.Customer.TeamMemberOfSaleman) equals Convert.ToInt32(c.Id)
                              select new
                              {
                                  CustomerId = c.Id,
                                  OrderTotal = oq.OrderTotal,
                                  OrderMonth = oq.CreatedOnUtc.Month,
                                  OrderMonthYear = oq.CreatedOnUtc.Month.ToString() + "/" + oq.CreatedOnUtc.Year.ToString(),
                                  profit = this.ProfitReport(OrderList, PItemList, oq.Id, pdIds),
                                  CommissionPercentage = c.CommissionPercentage,
                                  BasicSalary = c.BasicSalary
                              }).ToList();

            //******************************   PREPARE REPORT DATA ***********************************************//
            report = (from oq in refinedQry
                      group oq by new { oq.OrderMonth, oq.OrderMonthYear, oq.BasicSalary, oq.CommissionPercentage }
                      into profile
                      select new
                      {
                          Month = profile.Key,
                          MonthName = profile.Key.OrderMonthYear,
                          TotalOrders = profile.Count(),
                          CommissionPercentage = profile.Key.CommissionPercentage,
                          BasicSalary = profile.Key.BasicSalary,
                          TotalSale = profile.Sum(o => o.OrderTotal),
                          TotalProfit = profile.Sum(o => o.profit),
                          UniqueCustomers = profile.Select(c => new { c.CustomerId }).Distinct().ToList().Count
                      })
                .OrderByDescending(x => x.TotalOrders)
                .Select(r => new OrderBySalemanStatisticsReportLine
                {
                    MonthName = r.MonthName,
                    TotalSale = r.TotalSale,
                    Order = r.TotalOrders,
                    Commission = Math.Round((r.TotalSale * (r.CommissionPercentage ?? 0 / 100)), 2),
                    TotalProfit = Math.Round(r.TotalProfit, 2),
                    UniqueCustomers = r.UniqueCustomers,
                }).ToList();
            //  }

            return report;
        }
        /// <summary>
        /// Get "order by country" report
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="CustomerId">CustomerId</param>
        /// <param name="InvoiceNo">Invoice No</param>       
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <returns>Result</returns>
        public virtual IList<OrderByCustomerProfitReportLine> GetCustomerProfitReport(int storeId, int CustomerId, int OrderStatusId, string InvoiceNo, DateTime? startTimeUtc, DateTime? endTimeUtc)
        {

            var report = new List<OrderByCustomerProfitReportLine>();
            var pdIds = new List<int>();
            pdIds.Add((int)Nop.Core.Domain.Payments.PaymentStatus.Paid);
            var query = _orderRepository.Table;
            var CustomerRepo = _customerRepository.Table;
            var queryItems = _orderItemRepository.Table;
            if (_workContext.CurrentCustomer.IsSaleman())
            {
                query = query.Where(o => o.Customer.TeamMemberOfSaleman == _workContext.CurrentCustomer.Id);
            }
            query = query.Where(o => !o.Deleted);
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (CustomerId > 0)
                query = query.Where(o => o.CustomerId == CustomerId);
            // Dates are manadatory, bc auto assigned
            if (OrderStatusId > 0)
                query = query.Where(o => o.OrderStatusId == (OrderStatusId != 0 ? OrderStatusId : o.OrderStatusId));
            if (startTimeUtc.HasValue)
                query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
            if (endTimeUtc.HasValue)
                query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);

            var OrderList = (from oq in query
                             join c in CustomerRepo on oq.CustomerId equals c.Id
                             where oq.CustomerId > 0
                             select oq
                              ).ToList();

            //******************************  Customers Get All Respective Order Items***********************************************//
            var PItemList = (from orderItem in queryItems
                             join o in OrderList on orderItem.OrderId equals o.Id
                             select orderItem).ToList();

            //******************************  Customers Bio Data ***********************************************//
            var refinedQry = (from oq in query
                              join c in CustomerRepo on oq.CustomerId equals c.Id
                              select new
                              {
                                  OrderId = oq.Id,
                                  OrderTotal = oq.OrderTotal,
                                  Profit = this.ProfitReport(OrderList, PItemList, oq.Id, pdIds),
                                  ProfitTotal = this.ProfitReport(OrderList, PItemList, oq.Id, null),
                                  CustomerId = c.Id

                              }).ToList();

            //******************************   PREPARE REPORT DATA ***********************************************//
            report = (from gq in refinedQry
                      group gq by gq.CustomerId into q
                      select new
                      {
                          CustomerId = q.Key,
                          TotalOrders = q.Count(),
                          TotalSale = q.Sum(o => o.OrderTotal),
                          Profit = q.Sum(o => o.Profit),
                          ProfitTotal = q.Sum(o => o.ProfitTotal)

                      })
                .OrderByDescending(x => x.TotalOrders)
                .Select(r => new OrderByCustomerProfitReportLine
                {
                    CustomerId = r.CustomerId,
                    TotalSale = r.TotalSale,
                    InvoiceCount = r.TotalOrders,
                    Profit = Math.Round(r.Profit, 2),
                    ProfitTotal = Math.Round(r.ProfitTotal, 2)
                }).ToList();
            //  }

            return report;
        }
        /// <summary>SalemanCommissionReportModel
        /// Get order average report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="osIds">Order status identifiers</param>
        /// <param name="psIds">Payment status identifiers</param>
        /// <param name="ssIds">Shipping status identifiers</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <returns>Result</returns>
        public virtual OrderAverageReportLine GetOrderAverageReportLine(int storeId = 0,
            int vendorId = 0, int productId = 0, int warehouseId = 0, int billingCountryId = 0,
            int orderId = 0, string paymentMethodSystemName = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "", string orderNotes = null)
        {
            return GetOrderAverageReportLine2(storeId, vendorId, productId, warehouseId, billingCountryId, orderId, paymentMethodSystemName,
             osIds, psIds, ssIds, startTimeUtc, endTimeUtc, billingPhone, billingEmail, billingLastName, orderNotes, 0);
        }

        public virtual OrderAverageReportLine GetOrderAverageReportLine2(int storeId = 0,
            int vendorId = 0, int productId = 0, int warehouseId = 0, int billingCountryId = 0,
            int orderId = 0, string paymentMethodSystemName = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "", string orderNotes = null, int salesmanId = 0)
        {
            var query = _orderRepository.Table;
            query = query.Where(o => !o.Deleted);
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (salesmanId > 0)
                query = query.Where(o => o.SalemanId == salesmanId);
            if (orderId > 0)
                query = query.Where(o => o.Id == orderId);
            if (vendorId > 0)
                query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.Product.VendorId == vendorId));
            if (productId > 0)
                query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.ProductId == productId));

            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem =>
                        //"Use multiple warehouses" enabled
                        //we search in each warehouse
                        orderItem.Product.ManageInventoryMethodId == manageStockInventoryMethodId &&
                        orderItem.Product.UseMultipleWarehouses &&
                        orderItem.Product.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId)
                        ||
                        //"Use multiple warehouses" disabled
                        //we use standard "warehouse" property
                        (orderItem.Product.ManageInventoryMethodId != manageStockInventoryMethodId ||
                        !orderItem.Product.UseMultipleWarehouses) &&
                        orderItem.Product.WarehouseId == warehouseId));
            }

            if (billingCountryId > 0)
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);
            if (!string.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);
            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));
            if (startTimeUtc.HasValue)
                query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
            if (endTimeUtc.HasValue)
                query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);
            if (!string.IsNullOrEmpty(billingPhone))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.PhoneNumber) && o.BillingAddress.PhoneNumber.Contains(billingPhone));
            if (!string.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            if (!string.IsNullOrEmpty(billingLastName))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName));
            if (!string.IsNullOrEmpty(orderNotes))
                query = query.Where(o => o.OrderNotes.Any(on => on.Note.Contains(orderNotes)));

            var item = (from oq in query
                        group oq by 1
                into result
                        select new
                        {
                            OrderCount = result.Count(),
                            OrderShippingExclTaxSum = result.Sum(o => o.OrderShippingExclTax),
                            OrderPaymentFeeExclTaxSum = result.Sum(o => o.PaymentMethodAdditionalFeeExclTax),
                            OrderTaxSum = result.Sum(o => o.OrderTax),
                            OrderTotalSum = result.Sum(o => o.OrderTotal),
                            OrederRefundedAmountSum = result.Sum(o => o.RefundedAmount),
                            ActualShippingCost = result.Sum(o => o.ActualShippingCost),
                            NetProfitSum = result.Sum(o => o.Profit),
                            GrossProfitSum = result.Sum(o => o.GrossProfit)
                        }).Select(r => new OrderAverageReportLine
                        {
                            CountOrders = r.OrderCount,
                            SumShippingExclTax = r.OrderShippingExclTaxSum,
                            OrderPaymentFeeExclTaxSum = r.OrderPaymentFeeExclTaxSum,
                            SumTax = r.OrderTaxSum,
                            SumOrders = r.OrderTotalSum,
                            SumRefundedAmount = r.OrederRefundedAmountSum,
                            SumActualShippingCost = (r.ActualShippingCost.HasValue ? r.ActualShippingCost.Value : 0),
                            SumNetProfit = r.NetProfitSum.HasValue ? r.NetProfitSum.Value : 0,
                            SumGrossProfit = r.GrossProfitSum.HasValue ? r.NetProfitSum.Value : 0

                        }).FirstOrDefault();

            item = item ?? new OrderAverageReportLine
            {
                CountOrders = 0,
                SumShippingExclTax = decimal.Zero,
                OrderPaymentFeeExclTaxSum = decimal.Zero,
                SumTax = decimal.Zero,
                SumOrders = decimal.Zero,
                SumActualShippingCost = decimal.Zero,
                SumNetProfit = decimal.Zero,
                SumGrossProfit = decimal.Zero
            };
            return item;
        }

        /// <summary>
        /// Get order average report
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="os">Order status</param>
        /// <returns>Result</returns>
        public virtual OrderAverageReportLineSummary OrderAverageReport(int storeId, OrderStatus os)
        {
            return OrderAverageReport2(storeId, os, 0);
        }

        public virtual OrderAverageReportLineSummary OrderAverageReport2(int storeId, OrderStatus os, int salesmanId = 0)
        {
            var item = new OrderAverageReportLineSummary
            {
                OrderStatus = os
            };
            var orderStatuses = new List<int> { (int)os };

            var nowDt = _dateTimeHelper.ConvertToUserTime(DateTime.Now);
            var timeZone = _dateTimeHelper.CurrentTimeZone;

            //today
            var t1 = new DateTime(nowDt.Year, nowDt.Month, nowDt.Day);
            DateTime? startTime1 = _dateTimeHelper.ConvertToUtcTime(t1, timeZone);
            var todayResult = GetOrderAverageReportLine2(storeId,
                osIds: orderStatuses, salesmanId: salesmanId,
                startTimeUtc: startTime1);
            item.SumTodayOrders = todayResult.SumOrders;
            item.CountTodayOrders = todayResult.CountOrders;

            //week
            var fdow = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            var today = new DateTime(nowDt.Year, nowDt.Month, nowDt.Day);
            var t2 = today.AddDays(-(today.DayOfWeek - fdow));
            DateTime? startTime2 = _dateTimeHelper.ConvertToUtcTime(t2, timeZone);
            var weekResult = GetOrderAverageReportLine2(storeId,
                osIds: orderStatuses, salesmanId: salesmanId,
                startTimeUtc: startTime2);
            item.SumThisWeekOrders = weekResult.SumOrders;
            item.CountThisWeekOrders = weekResult.CountOrders;

            //month
            var t3 = new DateTime(nowDt.Year, nowDt.Month, 1);
            DateTime? startTime3 = _dateTimeHelper.ConvertToUtcTime(t3, timeZone);
            var monthResult = GetOrderAverageReportLine2(storeId,
                osIds: orderStatuses, salesmanId: salesmanId,
                startTimeUtc: startTime3);
            item.SumThisMonthOrders = monthResult.SumOrders;
            item.CountThisMonthOrders = monthResult.CountOrders;

            //year
            var t4 = new DateTime(nowDt.Year, 1, 1);
            DateTime? startTime4 = _dateTimeHelper.ConvertToUtcTime(t4, timeZone);
            var yearResult = GetOrderAverageReportLine2(storeId,
                osIds: orderStatuses, salesmanId: salesmanId,
                startTimeUtc: startTime4);
            item.SumThisYearOrders = yearResult.SumOrders;
            item.CountThisYearOrders = yearResult.CountOrders;

            //all time
            var allTimeResult = GetOrderAverageReportLine2(storeId, osIds: orderStatuses, salesmanId: salesmanId);
            item.SumAllTimeOrders = allTimeResult.SumOrders;
            item.CountAllTimeOrders = allTimeResult.CountOrders;

            return item;
        }

        /// <summary>
        /// Get best sellers report
        /// </summary>
        /// <param name="storeId">Store identifier (orders placed in a specific store); 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Shipping status; null to load all records</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all records</param>
        /// <param name="orderBy">1 - order by quantity, 2 - order by total amount</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Result</returns>
        public virtual IPagedList<BestsellersReportLine> BestSellersReport(
            int categoryId = 0, int manufacturerId = 0,
            int storeId = 0, int vendorId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            OrderStatus? os = null, PaymentStatus? ps = null, ShippingStatus? ss = null,
            int billingCountryId = 0,
            int orderBy = 1,
            int pageIndex = 0, int pageSize = int.MaxValue,
            bool showHidden = false)
        {
            return BestSellersReport2(categoryId: categoryId, manufacturerId: manufacturerId, storeId: storeId, vendorId: vendorId, createdFromUtc: createdFromUtc, createdToUtc: createdToUtc, os: os,
                ps: ps, ss: ss, billingCountryId: billingCountryId, orderBy: orderBy, pageIndex: pageIndex, pageSize: pageSize, showHidden: showHidden, salesmanId: 0);
        }
        public virtual IPagedList<BestsellersReportLine> BestSellersReport2(
            int categoryId = 0, int manufacturerId = 0,
            int storeId = 0, int vendorId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            OrderStatus? os = null, PaymentStatus? ps = null, ShippingStatus? ss = null,
            int billingCountryId = 0,
            int orderBy = 1,
            int pageIndex = 0, int pageSize = int.MaxValue,
            bool showHidden = false, int salesmanId = 0)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;

            var query1 = from orderItem in _orderItemRepository.Table
                         join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                         join p in _productRepository.Table on orderItem.ProductId equals p.Id
                         //join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId into p_pc from pc in p_pc.DefaultIfEmpty()
                         //join pm in _productManufacturerRepository.Table on p.Id equals pm.ProductId into p_pm from pm in p_pm.DefaultIfEmpty()
                         where (salesmanId == 0 || salesmanId == o.SalemanId) &&
                                (storeId == 0 || storeId == o.StoreId) &&
                               (!createdFromUtc.HasValue || createdFromUtc.Value <= o.CreatedOnUtc) &&
                               (!createdToUtc.HasValue || createdToUtc.Value >= o.CreatedOnUtc) &&
                               (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
                               (!paymentStatusId.HasValue || paymentStatusId == o.PaymentStatusId) &&
                               (!shippingStatusId.HasValue || shippingStatusId == o.ShippingStatusId) &&
                               !o.Deleted &&
                               !p.Deleted &&
                               (vendorId == 0 || p.VendorId == vendorId) &&
                               //(categoryId == 0 || pc.CategoryId == categoryId) &&
                               //(manufacturerId == 0 || pm.ManufacturerId == manufacturerId) &&
                               (categoryId == 0 || p.ProductCategories.Count(pc => pc.CategoryId == categoryId) > 0) &&
                               (manufacturerId == 0 || p.ProductManufacturers.Count(pm => pm.ManufacturerId == manufacturerId) >
                                0) &&
                               (billingCountryId == 0 || o.BillingAddress.CountryId == billingCountryId) &&
                               (showHidden || p.Published)
                         select orderItem;

            var query2 =
                //group by products
                from orderItem in query1
                group orderItem by orderItem.ProductId into g
                select new BestsellersReportLine
                {
                    ProductId = g.Key,
                    TotalAmount = g.Sum(x => x.PriceExclTax),
                    TotalQuantity = g.Sum(x => x.Quantity)
                };

            switch (orderBy)
            {
                case 1:
                    query2 = query2.OrderByDescending(x => x.TotalQuantity);
                    break;
                case 2:
                    query2 = query2.OrderByDescending(x => x.TotalAmount);
                    break;
                default:
                    throw new ArgumentException("Wrong orderBy parameter", "orderBy");
            }

            var result = new PagedList<BestsellersReportLine>(query2, pageIndex, pageSize);
            return result;
        }
        /// <summary>
        /// Gets a list of products (identifiers) purchased by other customers who purchased a specified product
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="recordsToReturn">Records to return</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Products</returns>
        public virtual int[] GetAlsoPurchasedProductsIds(int storeId, int productId,
            int recordsToReturn = 5, bool visibleIndividuallyOnly = true, bool showHidden = false)
        {
            if (productId == 0)
                throw new ArgumentException("Product ID is not specified");

            //this inner query should retrieve all orders that contains a specified product ID
            var query1 = from orderItem in _orderItemRepository.Table
                         where orderItem.ProductId == productId
                         select orderItem.OrderId;

            var query2 = from orderItem in _orderItemRepository.Table
                         join p in _productRepository.Table on orderItem.ProductId equals p.Id
                         where query1.Contains(orderItem.OrderId) &&
                         p.Id != productId &&
                         (showHidden || p.Published) &&
                         !orderItem.Order.Deleted &&
                         (storeId == 0 || orderItem.Order.StoreId == storeId) &&
                         !p.Deleted &&
                         (!visibleIndividuallyOnly || p.VisibleIndividually)
                         select new { orderItem, p };

            var query3 = from orderItem_p in query2
                         group orderItem_p by orderItem_p.p.Id into g
                         select new
                         {
                             ProductId = g.Key,
                             ProductsPurchased = g.Sum(x => x.orderItem.Quantity)
                         };
            query3 = query3.OrderByDescending(x => x.ProductsPurchased);

            if (recordsToReturn > 0)
                query3 = query3.Take(recordsToReturn);

            var report = query3.ToList();

            var ids = new List<int>();
            foreach (var reportLine in report)
                ids.Add(reportLine.ProductId);

            return ids.ToArray();
        }

        /// <summary>
        /// Gets a list of products that were never sold
        /// </summary>
        /// <param name="vendorId">Vendor identifier (filter products by a specific vendor); 0 to load all records</param>
        /// <param name="storeId">Store identifier (filter products by a specific store); 0 to load all records</param>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Products</returns>
        public virtual IPagedList<Product> ProductsNeverSold(int vendorId = 0, int storeId = 0,
            int categoryId = 0, int manufacturerId = 0,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            //this inner query should retrieve all purchased product identifiers
            var query_tmp = (from orderItem in _orderItemRepository.Table
                             join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                             where (!createdFromUtc.HasValue || createdFromUtc.Value <= o.CreatedOnUtc) &&
                                   (!createdToUtc.HasValue || createdToUtc.Value >= o.CreatedOnUtc) &&
                                   !o.Deleted
                             select orderItem.ProductId).Distinct();

            var simpleProductTypeId = (int)ProductType.SimpleProduct;

            var query = from p in _productRepository.Table
                        where !query_tmp.Contains(p.Id) &&
                              //include only simple products
                              p.ProductTypeId == simpleProductTypeId &&
                              !p.Deleted &&
                              (vendorId == 0 || p.VendorId == vendorId) &&
                              (categoryId == 0 || p.ProductCategories.Count(pc => pc.CategoryId == categoryId) > 0) &&
                              (manufacturerId == 0 || p.ProductManufacturers.Count(pm => pm.ManufacturerId == manufacturerId) > 0) &&
                              (showHidden || p.Published)
                        select p;

            if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
            {
                query = from p in query
                        join sm in _storeMappingRepository.Table
                        on new { c1 = p.Id, c2 = nameof(Product) } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into p_sm
                        from sm in p_sm.DefaultIfEmpty()
                        where !p.LimitedToStores || storeId == sm.StoreId
                        select p;
            }

            query = query.OrderBy(p => p.Name);

            var products = new PagedList<Product>(query, pageIndex, pageSize);
            return products;
        }

        /// <summary>
        /// Get profit report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier; pass 0 to ignore this parameter</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="osIds">Order status identifiers; null to load all records</param>
        /// <param name="psIds">Payment status identifiers; null to load all records</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all records</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <returns>Result</returns>
        public virtual decimal ProfitReport(int storeId = 0, int vendorId = 0, int productId = 0,
            int warehouseId = 0, int billingCountryId = 0, int orderId = 0, string paymentMethodSystemName = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "", string orderNotes = null)
        {
            var dontSearchPhone = string.IsNullOrEmpty(billingPhone);
            var dontSearchEmail = string.IsNullOrEmpty(billingEmail);
            var dontSearchLastName = string.IsNullOrEmpty(billingLastName);
            var dontSearchOrderNotes = string.IsNullOrEmpty(orderNotes);
            var dontSearchPaymentMethods = string.IsNullOrEmpty(paymentMethodSystemName);

            var orders = _orderRepository.Table;
            if (osIds != null && osIds.Any())
                orders = orders.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                orders = orders.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                orders = orders.Where(o => ssIds.Contains(o.ShippingStatusId));

            var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;

            var query = from orderItem in _orderItemRepository.Table
                        join o in orders on orderItem.OrderId equals o.Id
                        where (storeId == 0 || storeId == o.StoreId) &&
                              (orderId == 0 || orderId == o.Id) &&
                              (billingCountryId == 0 || (o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId)) &&
                              (dontSearchPaymentMethods || paymentMethodSystemName == o.PaymentMethodSystemName) &&
                              (!startTimeUtc.HasValue || startTimeUtc.Value <= o.CreatedOnUtc) &&
                              (!endTimeUtc.HasValue || endTimeUtc.Value >= o.CreatedOnUtc) &&
                              !o.Deleted &&
                              (vendorId == 0 || orderItem.Product.VendorId == vendorId) &&
                              (productId == 0 || orderItem.ProductId == productId) &&
                              (
                                warehouseId == 0
                                ||
                                //"Use multiple warehouses" enabled
                                //we search in each warehouse
                                orderItem.Product.ManageInventoryMethodId == manageStockInventoryMethodId &&
                                orderItem.Product.UseMultipleWarehouses &&
                                orderItem.Product.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId)
                                ||
                                //"Use multiple warehouses" disabled
                                //we use standard "warehouse" property
                                (orderItem.Product.ManageInventoryMethodId != manageStockInventoryMethodId ||
                                !orderItem.Product.UseMultipleWarehouses) &&
                                orderItem.Product.WarehouseId == warehouseId
                              ) &&
                              //we do not ignore deleted products when calculating order reports
                              //(!p.Deleted)
                              (dontSearchPhone || (o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.PhoneNumber) && o.BillingAddress.PhoneNumber.Contains(billingPhone))) &&
                              (dontSearchEmail || (o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail))) &&
                              (dontSearchLastName || (o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName))) &&
                              (dontSearchOrderNotes || o.OrderNotes.Any(oNote => oNote.Note.Contains(orderNotes)))
                        select orderItem;

            var productCost = Convert.ToDecimal(query.Sum(orderItem => (decimal?)orderItem.OriginalProductCost * orderItem.Quantity));

            var reportSummary = GetOrderAverageReportLine(
                storeId,
                vendorId,
                productId,
                warehouseId,
                billingCountryId,
                orderId,
                paymentMethodSystemName,
                osIds,
                psIds,
                ssIds,
                startTimeUtc,
                endTimeUtc,
                billingPhone,
                billingEmail,
                billingLastName,
                orderNotes);

            var profit = reportSummary.SumOrders
                         - reportSummary.SumShippingExclTax
                         - reportSummary.OrderPaymentFeeExclTaxSum
                         - reportSummary.SumTax
                         - reportSummary.SumRefundedAmount
                         - productCost;
            return profit;
        }

        /// <summary>
        /// Get profit report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier; pass 0 to ignore this parameter</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="osIds">Order status identifiers; null to load all records</param>
        /// <param name="psIds">Payment status identifiers; null to load all records</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all records</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <returns>Result</returns>
        public virtual decimal NetProfitReport(int storeId = 0, int vendorId = 0, int productId = 0,
            int warehouseId = 0, int billingCountryId = 0, int orderId = 0, string paymentMethodSystemName = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "", string orderNotes = null)
        {
            var dontSearchPhone = string.IsNullOrEmpty(billingPhone);
            var dontSearchEmail = string.IsNullOrEmpty(billingEmail);
            var dontSearchLastName = string.IsNullOrEmpty(billingLastName);
            var dontSearchOrderNotes = string.IsNullOrEmpty(orderNotes);
            var dontSearchPaymentMethods = string.IsNullOrEmpty(paymentMethodSystemName);

            var orders = _orderRepository.Table;
            if (osIds != null && osIds.Any())
                orders = orders.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                orders = orders.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                orders = orders.Where(o => ssIds.Contains(o.ShippingStatusId));

            var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;

            var query = from orderItem in _orderItemRepository.Table
                        join o in orders on orderItem.OrderId equals o.Id
                        where (storeId == 0 || storeId == o.StoreId) &&
                              (orderId == 0 || orderId == o.Id) &&
                              (billingCountryId == 0 || (o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId)) &&
                              (dontSearchPaymentMethods || paymentMethodSystemName == o.PaymentMethodSystemName) &&
                              (!startTimeUtc.HasValue || startTimeUtc.Value <= o.CreatedOnUtc) &&
                              (!endTimeUtc.HasValue || endTimeUtc.Value >= o.CreatedOnUtc) &&
                              !o.Deleted &&
                              (vendorId == 0 || orderItem.Product.VendorId == vendorId) &&
                              (productId == 0 || orderItem.ProductId == productId) &&
                              (
                                warehouseId == 0
                                ||
                                //"Use multiple warehouses" enabled
                                //we search in each warehouse
                                orderItem.Product.ManageInventoryMethodId == manageStockInventoryMethodId &&
                                orderItem.Product.UseMultipleWarehouses &&
                                orderItem.Product.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId)
                                ||
                                //"Use multiple warehouses" disabled
                                //we use standard "warehouse" property
                                (orderItem.Product.ManageInventoryMethodId != manageStockInventoryMethodId ||
                                !orderItem.Product.UseMultipleWarehouses) &&
                                orderItem.Product.WarehouseId == warehouseId
                              ) &&
                              //we do not ignore deleted products when calculating order reports
                              //(!p.Deleted)
                              (dontSearchPhone || (o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.PhoneNumber) && o.BillingAddress.PhoneNumber.Contains(billingPhone))) &&
                              (dontSearchEmail || (o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail))) &&
                              (dontSearchLastName || (o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName))) &&
                              (dontSearchOrderNotes || o.OrderNotes.Any(oNote => oNote.Note.Contains(orderNotes)))
                        select orderItem;

            var productCost = Convert.ToDecimal(query.Sum(orderItem => (decimal?)orderItem.OriginalProductCost * orderItem.Quantity));

            var reportSummary = GetOrderAverageReportLine(
                storeId,
                vendorId,
                productId,
                warehouseId,
                billingCountryId,
                orderId,
                paymentMethodSystemName,
                osIds,
                psIds,
                ssIds,
                startTimeUtc,
                endTimeUtc,
                billingPhone,
                billingEmail,
                billingLastName,
                orderNotes);

            var profit = reportSummary.SumOrders
                         //- reportSummary.SumShippingExclTax
                         - reportSummary.SumActualShippingCost
                         - reportSummary.OrderPaymentFeeExclTaxSum
                         - reportSummary.SumTax
                         - reportSummary.SumRefundedAmount
                         - productCost;
            return profit;
        }
        /// <summary>
        /// Get profit report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier; pass 0 to ignore this parameter</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="osIds">Order status identifiers; null to load all records</param>
        /// <param name="psIds">Payment status identifiers; null to load all records</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all records</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <returns>Result</returns>
        public virtual decimal ProfitReport(List<Order> query, List<OrderItem> pItems, int OrderId = 0, List<int> psIds = null)
        {


            //if (psIds != null && psIds.Any())
            //    query = query.Where(o => psIds.Contains(o.PaymentStatusId));

            var queryItem = from orderItem in pItems
                            join o in query on orderItem.OrderId equals o.Id
                            where o.Id == OrderId && (psIds != null && psIds.Any() ? psIds.Contains(o.PaymentStatusId) : true)
                            select orderItem;

            //  var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;

            var productCost = Convert.ToDecimal(queryItem.Sum(orderItem => (decimal?)orderItem.OriginalProductCost * orderItem.Quantity));

            var item = (from oq in query
                        where oq.Id == OrderId && (psIds != null && psIds.Any() ? psIds.Contains(oq.PaymentStatusId) : true)
                        group oq by 1
                 into result
                        select new
                        {
                            OrderCount = result.Count(),
                            OrderShippingExclTaxSum = result.Sum(o => o.OrderShippingExclTax),
                            OrderPaymentFeeExclTaxSum = result.Sum(o => o.PaymentMethodAdditionalFeeExclTax),
                            OrderTaxSum = result.Sum(o => o.OrderTax),
                            OrderTotalSum = result.Sum(o => o.OrderTotal),
                            OrederRefundedAmountSum = result.Sum(o => o.RefundedAmount),
                        }).Select(r => new OrderAverageReportLine
                        {
                            CountOrders = r.OrderCount,
                            SumShippingExclTax = r.OrderShippingExclTaxSum,
                            OrderPaymentFeeExclTaxSum = r.OrderPaymentFeeExclTaxSum,
                            SumTax = r.OrderTaxSum,
                            SumOrders = r.OrderTotalSum,
                            SumRefundedAmount = r.OrederRefundedAmountSum
                        }).FirstOrDefault();

            item = item ?? new OrderAverageReportLine
            {
                CountOrders = 0,
                SumShippingExclTax = decimal.Zero,
                OrderPaymentFeeExclTaxSum = decimal.Zero,
                SumTax = decimal.Zero,
                SumOrders = decimal.Zero
            };


            var profit = item.SumOrders
                         - item.SumShippingExclTax
                         - item.OrderPaymentFeeExclTaxSum
                         - item.SumTax
                         - item.SumRefundedAmount
                         - productCost;
            return profit;
        }
        #endregion
    }
}