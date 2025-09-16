using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Payments;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using static Nop.Web.Areas.Admin.Models.Customers.CustomerModel;

namespace Nop.Web.Areas.Admin.Models.Orders
{
    /// <summary>
    /// Represents an order model
    /// </summary>
    public partial class OrderModel : BaseNopEntityModel
    {
        #region Ctor

        public OrderModel()
        {
            CustomValues = new Dictionary<string, object>();
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentTerms = new List<SelectListItem>();            
            AvailableCategories = new List<SelectListItem>();
            TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
            Items = new List<OrderItemModel>();
            UsedDiscounts = new List<UsedDiscountModel>();
            OrderShipmentSearchModel = new OrderShipmentSearchModel();
            OrderNoteSearchModel = new OrderNoteSearchModel();
            BillingAddress = new AddressModel();
            ShippingAddress = new AddressModel();
            PickupAddress = new AddressModel();
        }

        #endregion

        #region Properties

        public bool IsLoggedInAsVendor { get; set; }

        //identifiers
        public override int Id { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.OrderGuid")]
        public Guid OrderGuid { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.CustomOrderNumber")]
        public string CustomOrderNumber { get; set; }

        //store
        [NopResourceDisplayName("Admin.Orders.Fields.Store")]
        public string StoreName { get; set; }

        //customer info
        [NopResourceDisplayName("Admin.Orders.Fields.Customer")]
        public int CustomerId { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Customer")]
        public string CustomerInfo { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.CustomerEmail")]
        public string CustomerEmail { get; set; }
        public string CustomerFullName { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.CustomerIP")]
        public string CustomerIp { get; set; }

        [NopResourceDisplayName("Admin.Orders.Fields.CustomValues")]
        public Dictionary<string, object> CustomValues { get; set; }

        [NopResourceDisplayName("Admin.Orders.Fields.Affiliate")]
        public int AffiliateId { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Affiliate")]
        public string AffiliateName { get; set; }

        //Used discounts
        [NopResourceDisplayName("Admin.Orders.Fields.UsedDiscounts")]
        public IList<UsedDiscountModel> UsedDiscounts { get; set; }

        //totals
        public bool AllowCustomersToSelectTaxDisplayType { get; set; }
        public TaxDisplayType TaxDisplayType { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.OrderSubtotalInclTax")]
        public string OrderSubtotalInclTax { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.OrderSubtotalExclTax")]
        public string OrderSubtotalExclTax { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.OrderSubTotalDiscountInclTax")]
        public string OrderSubTotalDiscountInclTax { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.OrderSubTotalDiscountExclTax")]
        public string OrderSubTotalDiscountExclTax { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.OrderShippingInclTax")]
        public string OrderShippingInclTax { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.OrderShippingExclTax")]
        public string OrderShippingExclTax { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.PaymentMethodAdditionalFeeInclTax")]
        public string PaymentMethodAdditionalFeeInclTax { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.PaymentMethodAdditionalFeeExclTax")]
        public string PaymentMethodAdditionalFeeExclTax { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Tax")]
        public string Tax { get; set; }
        public IList<TaxRate> TaxRates { get; set; }
        public bool DisplayTax { get; set; }
        public bool DisplayTaxRates { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.OrderTotalDiscount")]
        public string OrderTotalDiscount { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.RedeemedRewardPoints")]
        public int RedeemedRewardPoints { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.RedeemedRewardPoints")]
        public string RedeemedRewardPointsAmount { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.OrderTotal")]
        public string OrderTotal { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.RefundedAmount")]
        public string RefundedAmount { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.NetProfit")]
        public string NetProfit { get; set; }

        //edit totals
        [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderSubtotal")]
        public decimal OrderSubtotalInclTaxValue { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderSubtotal")]
        public decimal OrderSubtotalExclTaxValue { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderSubTotalDiscount")]
        public decimal OrderSubTotalDiscountInclTaxValue { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderSubTotalDiscount")]
        public decimal OrderSubTotalDiscountExclTaxValue { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderShipping")]
        public decimal OrderShippingInclTaxValue { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderShipping")]
        public decimal OrderShippingExclTaxValue { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Edit.PaymentMethodAdditionalFee")]
        public decimal PaymentMethodAdditionalFeeInclTaxValue { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Edit.PaymentMethodAdditionalFee")]
        public decimal PaymentMethodAdditionalFeeExclTaxValue { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Edit.Tax")]
        public decimal TaxValue { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Edit.TaxRates")]
        public string TaxRatesValue { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderTotalDiscount")]
        public decimal OrderTotalDiscountValue { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.Edit.OrderTotal")]
        public decimal OrderTotalValue { get; set; }

        //associated recurring payment id
        [NopResourceDisplayName("Admin.Orders.Fields.RecurringPayment")]
        public int RecurringPaymentId { get; set; }

        //order status
        [NopResourceDisplayName("Admin.Orders.Fields.OrderStatus")]
        public string OrderStatus { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.OrderStatus")]
        public int OrderStatusId { get; set; }

        //payment info
        [NopResourceDisplayName("Admin.Orders.Fields.PaymentStatus")]
        public string PaymentStatus { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.PaymentStatus")]
        public int PaymentStatusId { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.PaymentMethod")]
        public string PaymentMethod { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.PaymentTerm")]
        public int PaymentTermId { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.NoteForWarehouse")]
        public string NoteForWarehouse { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.NoteForAccounts")]
        public string NoteForAccounts { get; set; }

        //credit card info
        public bool AllowStoringCreditCardNumber { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.CardType")]
        public string CardType { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.CardName")]
        public string CardName { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.CardNumber")]
        public string CardNumber { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.CardCVV2")]
        public string CardCvv2 { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.CardExpirationMonth")]
        public string CardExpirationMonth { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.CardExpirationYear")]
        public string CardExpirationYear { get; set; }

        //misc payment info
        [NopResourceDisplayName("Admin.Orders.Fields.AuthorizationTransactionID")]
        public string AuthorizationTransactionId { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.CaptureTransactionID")]
        public string CaptureTransactionId { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.SubscriptionTransactionID")]
        public string SubscriptionTransactionId { get; set; }

        //shipping info
        public bool IsShippable { get; set; }
        public bool PickupInStore { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.PickupAddress")]
        public AddressModel PickupAddress { get; set; }
        public string PickupAddressGoogleMapsUrl { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.ShippingStatus")]
        public string ShippingStatus { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.ShippingStatus")]
        public int ShippingStatusId { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.ShippingAddress")]
        public AddressModel ShippingAddress { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.ShippingMethod")]
        public string ShippingMethod { get; set; }
        public string ShippingAddressGoogleMapsUrl { get; set; }
        public bool CanAddNewShipments { get; set; }
        [NotMapped]
        public Order order { get; set; }
        [NotMapped]
        public int ItemQuantity { get; set; }
        [NotMapped]
        [NopResourceDisplayName("Admin.Orders.Fields.CustomShippingCharges")]
        public decimal? CustomShippingCharges{ get; set; }

        [NotMapped]
        public IList<SelectListItem> PaymentMethodList { get; set; }

        [NotMapped]
        public IList<SelectListItem> AvailablePaymentTerms { get; set; }

        [NotMapped]
        [NopResourceDisplayName("Admin.Orders.PaymentDueOn")]
        public string PaymentDueOn { get; set; }

        [NotMapped]
        [NopResourceDisplayName("Admin.Orders.Fields.ShippingAddress")]
        public int ShippingAddressId { get; set; }
        [NotMapped]
        public int cart { get; set; }
        //billing info
        [NopResourceDisplayName("Admin.Orders.Fields.BillingAddress")]
        public AddressModel BillingAddress { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.VatNumber")]
        public string VatNumber { get; set; }
        // Start Quick Order
        public bool DateInvoiceEnabled { get; set; } = true;
        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.DateInvoice")]
        public DateTime? DateInvoice { get; set; } = System.DateTime.Now;
        //SalesMan      
        [NopResourceDisplayName("Admin.Orders.List.Saleman")]
        public int SalemanId { get; set; }
        //[NopResourceDisplayName("Salesman")]
      
        //[NopResourceDisplayName("Admin.Customers.Customers.Fields.CustomerRoles")]
        //  public IList<SelectListItem> AvailablePriceGroup { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.PriceGroup")]
        public int PriceGroupId { get; set; } 
        //Customer Role
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.CustomerRoles")]
        public string CustomerRoleNames { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.PriceGroup")]
        public List<int> CustomerRoleId { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.CustomerRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        //Remarks
        public bool RemarksEnabled { get; set; } = true;
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Remarks")]
        //[NopResourceDisplayName("Remarks")]
        public string Remarks { get; set; }
        //order Status
        // public bool IsLoggedInAsVendor { get; set; }
        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        [NopResourceDisplayName("Admin.Orders.List.OrderStatus")]
        public IList<int> OrderStatusIds { get; set; }

        [NopResourceDisplayName("Admin.Orders.Fields.QuickOrderProductCategory")]
        public int ProductCategory { get; set; }

        public IList<SelectListItem> ProductCategoryList { get; set; }
        // Saleman
        public IList<CustomerSalemanListItem> SalemanList { get; set; }
        // Customers
        public IList<CustomerSalemanListItem> CustomersList { get; set; }
        //Packing Type
        public IList<SelectListItem> PackingTypes { get; set; }
        // Price Group
        public IList<SelectListItem> PriceGroupsList { get; set; }
        [NopResourceDisplayName("Admin.Orders.List.Admin.Orders.List.PackingType")]
        public int PackingTypeId { get; set; }
        //Summery
        public bool NetDiscountEnabled { get; set; } = true;
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.NetDiscount")]
        //[NopResourceDisplayName("Net Discount")]
        public string NetDiscount { get; set; }
        public bool BillAmoutEnabled { get; set; } = true;
        // [NopResourceDisplayName("Admin.Customers.Customers.Fields.BillAmount")]
        [NopResourceDisplayName("Admin.Orders.QuickOrder.Fields.BillAmount")]
        public string BillAmount { get; set; }
        public bool PaidAmountEnabled { get; set; } = true;
        //[NopResourceDisplayName("Admin.Customers.Customers.Fields.PaidAmount")]
        [NopResourceDisplayName("Admin.Orders.QuickOrder.Fields.PaidAmount")]
        public string PaidAmount { get; set; }
        public bool DueBalanceEnabled { get; set; } = true;
        //[NopResourceDisplayName("Admin.Orders.QuickOrder.Fields.DueBalance")]
        [NopResourceDisplayName("Admin.Orders.QuickOrder.Fields.DueBalance")]
        public string DueBalance { get; set; }
        //****************Column************
        public bool BrandEnabled { get; set; } = true;
        //[NopResourceDisplayName("Admin.Orders.Fields.Brand")]
        [NopResourceDisplayName("Brand")]
        public IList<int> Brand { get; set; }
        public bool CodeEnabled { get; set; } = true;
        //[NopResourceDisplayName("Admin.Orders.Fields.Code")]
        [NopResourceDisplayName("Admin.Orders.Fields.ProductSKU")]
        public IList<int> Code { get; set; }
        public bool QtyEnabled { get; set; } = true;
        //[NopResourceDisplayName("Admin.Customers.Customers.Fields.Brand")]
        [NopResourceDisplayName("Admin.Orders.Fields.Qty")]
        public IList<int> Qty { get; set; }
        public bool PriceEnabled { get; set; } = true;
        //[NopResourceDisplayName("Admin.Customers.Customers.Fields.Brand")]
        [NopResourceDisplayName("Admin.Orders.Fields.QuickOrderPrice")]
        public IList<int> Price { get; set; }
        public bool DiscountEnabled { get; set; } = true;
        //[NopResourceDisplayName("Admin.Customers.Customers.Fields.Brand")]
        [NopResourceDisplayName("Admin.Orders.Fields.QuickOrderDiscount")]
        public IList<int> Discount { get; set; }
        public bool TotalEnabled { get; set; } = true;
        //[NopResourceDisplayName("Admin.Customers.Customers.Fields.Brand")]
        [NopResourceDisplayName("Admin.Orders.Fields.QuickOrderTotal")]
        public IList<int> Total { get; set; }       
        public IList<int> SelectedAvailableProductIds { get; set; }
        public bool ActionEnabled { get; set; } = true;
        //[NopResourceDisplayName("Admin.Customers.Customers.Fields.Brand")]
        [NopResourceDisplayName("Action")]
        public IList<int> Action { get; set; }
        public IList<SelectListItem> AvailableProducts { get; set; }
        public IList<SelectListItem> AvailablePackingTypes { get; set; }

        public bool IsQuickOrder { get; set; }
        public bool GdprEnabled { get; set; }
        public SendPmModel SendPm { get; set; }
        public SendEmailModel SendEmail { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public partial class SendPmModel : BaseNopModel
        {
            [NopResourceDisplayName("Admin.Customers.Customers.SendPM.Subject")]
            public string Subject { get; set; }

            [NopResourceDisplayName("Admin.Customers.Customers.SendPM.Message")]
            public string Message { get; set; }
        }
        public partial class SendEmailModel : BaseNopModel
        {
            [NopResourceDisplayName("Admin.Customers.Customers.SendEmail.Subject")]
            public string Subject { get; set; }

            [NopResourceDisplayName("Admin.Customers.Customers.SendEmail.Body")]
            public string Body { get; set; }

            [NopResourceDisplayName("Admin.Customers.Customers.SendEmail.SendImmediately")]
            public bool SendImmediately { get; set; }

            [NopResourceDisplayName("Admin.Customers.Customers.SendEmail.DontSendBeforeDate")]
            [UIHint("DateTimeNullable")]
            public DateTime? DontSendBeforeDate { get; set; }
        }


        //End Quick Order        
        //gift cards
        public IList<GiftCard> GiftCards { get; set; }

        //items
        public bool HasDownloadableProducts { get; set; }
        public IList<OrderItemModel> Items { get; set; }

        //creation date
        [NopResourceDisplayName("Admin.Orders.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        //checkout attributes
        public string CheckoutAttributeInfo { get; set; }

        //order notes
        [NopResourceDisplayName("Admin.Orders.OrderNotes.Fields.DisplayToCustomer")]
        public bool AddOrderNoteDisplayToCustomer { get; set; }
        [NopResourceDisplayName("Admin.Orders.OrderNotes.Fields.Note")]
        public string AddOrderNoteMessage { get; set; }
        public bool AddOrderNoteHasDownload { get; set; }
        [NopResourceDisplayName("Admin.Orders.OrderNotes.Fields.Download")]
        [UIHint("Download")]
        public int AddOrderNoteDownloadId { get; set; }

        //refund info
        [NopResourceDisplayName("Admin.Orders.Fields.PartialRefund.AmountToRefund")]
        public decimal AmountToRefund { get; set; }
        public decimal MaxAmountToRefund { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }

        //workflow info
        public bool CanCancelOrder { get; set; }
        public bool CanCapture { get; set; }
        public bool CanMarkOrderAsPaid { get; set; }
        public bool CanRefund { get; set; }
        public bool CanRefundOffline { get; set; }
        public bool CanPartiallyRefund { get; set; }
        public bool CanPartiallyRefundOffline { get; set; }
        public bool CanVoid { get; set; }
        public bool CanVoidOffline { get; set; }

        public OrderShipmentSearchModel OrderShipmentSearchModel { get; set; }

        public OrderNoteSearchModel OrderNoteSearchModel { get; set; }

        [NopResourceDisplayName("Admin.Orders.Fields.ReferneceNo")]
        public string ReferneceNo { get; set; }

        [NopResourceDisplayName("Admin.Orders.Fields.GrossProfit")]
        public string GrossProfit { get; set; }
        [NopResourceDisplayName("Admin.Orders.Fields.ActualShippingCost")]
        public decimal ActualShippingCost { get; set; }

        [NopResourceDisplayName("Admin.Orders.Fields.ActualShippingCost")]
        public string ActualShippingCostStr { get; set; }
        #endregion

        #region Nested Classes

        public partial class TaxRate : BaseNopModel
        {
            public string Rate { get; set; }
            public string Value { get; set; }
        }

        public partial class GiftCard : BaseNopModel
        {
            [NopResourceDisplayName("Admin.Orders.Fields.GiftCardInfo")]
            public string CouponCode { get; set; }
            public string Amount { get; set; }
        }

        public partial class UsedDiscountModel : BaseNopModel
        {
            public int DiscountId { get; set; }
            public string DiscountName { get; set; }
        }

        #endregion
    }

    public partial class OrderAggreratorModel : BaseNopModel
    {
        //aggergator properties
        public string aggregatorprofit { get; set; }
        public string aggregatorshipping { get; set; }
        public string aggregatortax { get; set; }
        public string aggregatortotal { get; set; }
    }
}