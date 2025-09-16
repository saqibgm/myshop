using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Weclapp.model
{
    public class WProduct
    {
        public string id { get; set; } = "";
        public int batchId { get; set; }
        public string nopid { get; set; } = "";
        public string version { get; set; } = "";
        public bool active { get; set; } = true;
        public bool applyCashDiscount { get; set; } = false;
        public List<object> articleAlternativeQuantities { get; set; }
        public List<object> articleCalculationPrices { get; set; }
        public List<object> articleImages { get; set; }
        public string articleNumber { get; set; } = "";
        public string ean { get; set; } = "";
        public List<WProductPrice> articlePrices { get; set; }
        public string articleType { get; set; } = Configs.ArticleType;
        public List<object> availableForSalesChannels { get; set; }
        public bool availableInSale { get; set; } = true;
        public bool batchNumberRequired { get; set; } = false;
        public bool billOfMaterialPartDeliveryPossible { get; set; } = false;
        public long createdDate { get; set; }
        public List<object> customAttributes { get; set; }
        public List<object> customerArticleNumbers { get; set; }
        public string defaultPriceCalculationType { get; set; } = Configs.PriceCalculationType;
        public List<object> defaultStoragePlaces { get; set; }
        public bool defineIndividualTaskTemplates { get; set; } = false;
        public long lastModifiedDate { get; set; }
        public int lowLevelCode { get; set; }
        public string marginCalculationPriceType { get; set; } = Configs.MarginCalculationPriceType;
        public string name { get; set; } = "";
        public string packagingUnitBaseArticleId { get; set; } = "";
        public List<object> priceCalculationParameters { get; set; }
        public bool productionArticle { get; set; } = false;
        public List<object> productionBillOfMaterialItems { get; set; }
        public string productionConfigurationRule { get; set; } = Configs.ProductionConfigurationRule;
        public List<object> quantityConversions { get; set; }
        public List<object> salesBillOfMaterialItems { get; set; }
        public bool serialNumberRequired { get; set; } = false;
        public bool showOnDeliveryNote { get; set; } = false;
        public List<object> supplySources { get; set; }
        public List<object> tags { get; set; }
        public string taxRateType { get; set; } = Configs.Standard;
        public string unitId { get; set; } = Configs.UnitId;
        public string unitName { get; set; } = "pc.";
        public bool useAvailableForSalesChannels { get; set; } = false;
        public bool useSalesBillOfMaterialItemPrices { get; set; } = false;
        public bool useSalesBillOfMaterialItemPricesForPurchase { get; set; } = false;
        public bool useSalesBillOfMaterialSubitemCosts { get; set; } = false;
    }
    public class WProductPrice
    {
        public string id { get; set; }
        public string version { get; set; }
        public long createdDate { get; set; }
        public string currencyId { get; set; } = Configs.CurrencyId;
        public string currencyName { get; set; } = Configs.CurrencyName;
        public string lastModifiedByUserId { get; set; }
        public long lastModifiedDate { get; set; }
        public decimal price { get; set; }
        public string priceType { get; set; } = Configs.PriceType;
        public string priceScaleType { get; set; } = Configs.PriceScaleType;
        public string priceScaleValue { get; set; } = "1";
        public List<object> reductionAdditions { get; set; }
        public string salesChannel { get; set; } = Configs.SaleChannel;
    }
    public class WProductCustomFields
    {
        public string sku { get; set; }
    }

        public class NopProductAttribute
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class NopTierPrice
    {
        public int quantity { get; set; }
        public decimal price { get; set; }
    }
    public class NopProductPrice
    {
        public string currencyId { get; set; }
        public string priceType { get; set; } = Configs.PriceType;
        public string priceScaleType { get; set; } = Configs.PriceScaleType;
        public string priceScaleValue { get; set; } = Configs.ScaleValue;
        public string salesChannel { get; set; } = Configs.SaleChannel;
        public decimal price { get; set; }
    }

}

