using System;
using System.Collections.Generic;
using System.Text;
using static iTextSharp.text.pdf.AcroFields;

namespace Nop.Services.Weclapp.model
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);





    public class WOrder
    {
        public string id { get; set; } = "";
        public int batchId { get; set; }
        public string orderNumber { get; set; }
        public string nopid { get; set; }
        //public string customerOrderNumber { get; set; } = "";
        public string creatorId { get; set; } = "";
        public string customerId { get; set; } = "";
        public string customerNumber { get; set; } = "";
        public string currency { get; set; } = Configs.CurrencyName;
        public decimal totalPrice { get; set; }
        public string orderDate { get; set; } = "";
        public string createdDate { get; set; } = "";
        public string status { get; set; } = WOrderStatus.IN_PROGRESS;
        public string salesChannel { get; set; } = Configs.SaleChannel;
        public string salesOrderPaymentType { get; set; } = Configs.Standard;
        public bool template { get; set; } = false;
        public bool applyShippingCostsOnlyOnce { get; set; } = false;
        public bool recordOpeningInheritance { get; set; } = false;
        public bool recordFreeTextInheritance { get; set; } = false;
        public bool recordCommentInheritance { get; set; } = false;
        public bool factoring { get; set; } = false;
        public bool sentToRecipient { get; set; } = false;
        public bool disableEmailTemplate { get; set; } = false;
        public decimal grossAmount { get; set; }
        public decimal grossAmountInCompanyCurrency { get; set; }
        public decimal netAmount { get; set; }
        public decimal netAmountInCompanyCurrency { get; set; }
        public string availability { get; set; } = Configs.NotChecked;
        public string availabilityForAllWarehouses { get; set; } = Configs.NotChecked;
        public string currencyConversionDate { get; set; }
        public string currencyConversionRate { get; set; } = Configs.ScaleValue;
        public string dispatchCountryCode { get; set; } = "DE";
        public string fulfillmentProviderId { get; set; } = Configs.FulfillmentProviderID;

        public string headerDiscount { get; set; } = Configs.Zero;
        public string headerSurcharge { get; set; } = Configs.Zero;
        public bool invoiced { get; set; } = false;

        public bool paid { get; set; } = false;
        public string plannedShippingDate { get; set; }
        public string pricingDate { get; set; } = "";
        public bool projectModeActive { get; set; } = false;
        public string recordCurrencyId { get; set; } = Configs.CurrencyId;
        public string recordCurrencyName { get; set; } = Configs.CurrencyName;
        public string responsibleUserId { get; set; } = Configs.UserId;
        public string responsibleUserUsername { get; set; } = Configs.UserName;
        public bool servicesFinished { get; set; } = false;
        public bool shipped { get; set; } = false;
        public int shippingLabelsCount { get; set; } = 1;
        public string warehouseId { get; set; } = Configs.WarehouseId;
        public string warehouseName { get; set; } = Configs.WarehouseName;
        public List<WOrderItem> orderItems { get; set; }
        public List<WOrderItem> shippingCostItems { get; set; }
    }

    public class WOrderItem
    {
        public string id { get; set; } = "";
        public string nopid { get; set; } = "";
        public string title { get; set; } = "";
        public string articleId { get; set; } = "";
        public string articleNumber { get; set; } = "";
        public string quantity { get; set; } = "";
        public bool manualUnitPrice { get; set; } = true;
        public string unitId { get; set; } = Configs.UnitId;
        public string unitName { get; set; } = Configs.UnitName;
        public decimal unitPrice { get; set; }
        public bool addPageBreakBefore { get; set; } = false;
        public bool disableEmailTemplate { get; set; } = false;
        public bool descriptionFixed { get; set; } = false;
        public bool manualPlannedWorkingTimePerUnit { get; set; } = false;
        public bool manualUnitCost { get; set; } = false;
        public string discountPercentage { get; set; } = Configs.Zero;
        public string itemType { get; set; } = Configs.Default;
        public string taxId { get; set; } = Configs.TaxId;
        public decimal unitPriceInCompanyCurrency { get; set; }
        public string warehouseId { get; set; } = Configs.WarehouseId;
        public string warehouseName { get; set; } = Configs.WarehouseName;
        public decimal netAmount { get; set; }
        public decimal netAmountForStatistics { get; set; }
        public decimal netAmountForStatisticsInCompanyCurrency { get; set; }
        public decimal netAmountInCompanyCurrency { get; set; }
        public decimal grossAmount { get; set; }
        public decimal grossAmountInCompanyCurrency { get; set; }
        public string version { get; set; } = Configs.Zero;
        public string availability { get; set; } = Configs.NotChecked;
        public string availabilityForAllWarehouses { get; set; } = Configs.NotChecked;
        public string createdDate { get; set; } = "";
        public string invoicedQuantity { get; set; } = Configs.Zero;
        public string lastModifiedDate { get; set; } = "";
        public bool manualQuantity { get; set; } = true;
        public int positionNumber { get; set; } = 2;
        public bool shipped { get; set; } = false;
        public string shippedQuantity { get; set; } = Configs.Zero;
        public string taxName { get; set; } = Configs.TaxName;
        //public string taxRate { get; set; } = "";
        public string invoicingType { get; set; } 
    }

    public class NewOrder
    {
        public string id { get; set; } = "";
        public string nopid { get; set; } = "";
        public string version { get; set; } = Configs.Zero;
        public string customerId { get; set; } = "";
        public string customerNumber { get; set; } = "";
        public string currency { get; set; } = Configs.CurrencyName;
        public decimal totalPrice { get; set; }
        public string orderDate { get; set; } = "";
        public string createdDate { get; set; } = "";
        public string status { get; set; } = WOrderStatus.IN_PROGRESS;
        public string salesChannel { get; set; } = Configs.SaleChannel;
        public string salesOrderPaymentType { get; set; } = Configs.Standard;
        public bool template { get; set; } = false;
        public bool applyShippingCostsOnlyOnce { get; set; } = false;
        public bool recordOpeningInheritance { get; set; } = false;
        public bool recordFreeTextInheritance { get; set; } = false;
        public bool recordCommentInheritance { get; set; } = false;
        public bool factoring { get; set; } = false;
        public bool sentToRecipient { get; set; } = false;
        public bool disableEmailTemplate { get; set; } = false;
        public string orderNumber { get; set; } = "";
        public List<NewOrderItem> orderItems { get; set; }
        public List<NewOrderItem> shippingCostItems { get; set; }

    }
    public class NewOrderItem
    {
        public string id { get; set; } = "";
        public string nopid { get; set; } = "";
        public string articleId { get; set; } = "";
        public string articleNumber { get; set; } = "";
        public string quantity { get; set; } = "";
        public bool manualUnitPrice { get; set; } = true;
        //public string taxRate { get; set; } = "";
        public string unitId { get; set; } = Configs.UnitId;
        public string unitName { get; set; } = Configs.UnitName;
        public decimal unitPrice { get; set; }
        public bool addPageBreakBefore { get; set; } = false;
        public bool disableEmailTemplate { get; set; } = false;
        public bool descriptionFixed { get; set; } = false;
        public bool manualPlannedWorkingTimePerUnit { get; set; } = false;
        public bool manualUnitCost { get; set; } = false;
        public string discountPercentage { get; set; } = Configs.Zero;
        public string itemType { get; set; } = Configs.ItemType;
        public string taxId { get; set; } = Configs.TaxId;
        public string taxName { get; set; } = "";
        public decimal unitPriceInCompanyCurrency { get; set; }
        public string warehouseId { get; set; } = Configs.WarehouseId;
        public string warehouseName { get; set; } = Configs.WarehouseName;
        public string invoicingType { get; set; }
        
    }
    public static class WOrderStatus
    {
        public const string CANCELLED = "CANCELLED";
        public const string CLOSED = "CLOSED";
        public const string MANUALLY_CLOSED = "MANUALLY_CLOSED";
        public const string CONFIRMATION_PRINTED = "ORDER_CONFIRMATION_PRINTED";
        public const string IN_PROGRESS = "ORDER_ENTRY_IN_PROGRESS";
    }
    public static class WCurrency
    {
        public const string EURO = "EUR";
        public const string POUND = "GBP";
    }
    public static class WCountryCodes
    {
        public const string Garman = "DE";
        public const string UK = "UK";
    }

    public static class WPartyTypes
    {
        public const string PERSON = "PERSON";
        public const string ORGANIZATION = "ORGANIZATION";
    }
    public static class WEntityTypes
    {
        public const string PRODUCT = "Product";
        public const string CUSTOMER = "Customer";
        public const string ORDER = "Order";
    }
    public static class WActionTypes
    {
        public const string INSERT = "Insert Entity";
        public const string UPDATE = "Update Entity";
        public const string UPDATESTATUS = "Update Status";
    }
    
    public static class Configs
    {
        public const string NoTax = "0:0;";
        public static string CurrencyId { get; set; }
        public static string WarehouseId { get; set; }
        public static string WarehouseName { get; set; }
        public static string ItemType { get; set; } = "DEFAULT";
        public static string TaxId { get; set; }
        public static string TaxName { get; set; }
        public static string NoTaxId { get; set; }
        public static string NoTaxName { get; set; }
        public static string CurrencyName { get; set; } = WCurrency.EURO;
        public static string UnitId { get; set; }
        public static string UnitName { get; set; } = "pc.";
        public static string UserId { get; set; }
        public static string UserName { get; set; }
        public static string NotChecked { get; set; } = "NOT_CHECKED";
        public static string Standard { get; set; } = "STANDARD";
        public static string CustomerType { get; set; } = "CUSTOMER";
        public static string Default { get; set; } = "DEFAULT";
        public static string SaleChannel { get; set; } = "NET1";
        public static string Zero { get; set; } = "0";
        public static string ArticleType { get; set; } = "BASIC";
        public static string CountryCode { get; set; } = WCountryCodes.Garman;
        public static string FulfillmentProviderID { get; set; } = "STANDARD";
        public static string PriceCalculationType { get; set; } = "PURCHASE";
        public static string ProductionConfigurationRule { get; set; } = "AT_LEAST_ONE_COMPONENT";
        public static string PartyType { get; set; } = WPartyTypes.ORGANIZATION;
        public static string PriceType { get; set; } = "SALE_PRICE";
        public static string PriceScaleType { get; set; } = "SCALE_FROM";
        public static string ScaleValue { get; set; } = "1";
        public static string InvoicingTypeFixed { get; set; } = "FIXED_PRICE";
        public static string MarginCalculationPriceType { get; set; } = "PURCHASE_PRICE_PRODUCTION_COST";
        

    }
}