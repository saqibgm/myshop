using Microsoft.AspNetCore.Identity.UI.V3.Pages.Account.Internal;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Nop.Services.Weclapp.model
{
    public class WCustomer
    {
        public string id { get; set; } = "";
        public int batchId { get; set; }
        public string nopid { get; set; } = "";
        public string name { get; set; } = "";
        public string version { get; set; } = "";
        public List<WAddress> addresses { get; set; }
        public List<object> bankAccounts { get; set; }
        public bool blocked { get; set; } = false;
        public List<object> commissionSalesPartners { get; set; }
        public string company { get; set; } = "";
        public List<object> contacts { get; set; }
        public long createdDate { get; set; } = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        public string currencyId { get; set; } = "";
        public string currencyName { get; set; } = "";
        public List<object> customAttributes { get; set; }
        public string customerNumber { get; set; } = "";
        public List<object> customerTopics { get; set; }
        public bool deliveryBlock { get; set; } = false;
        public string email { get; set; } = "";
        public bool insolvent { get; set; } = false;
        public bool insured { get; set; } = false;
        public bool invoiceBlock { get; set; } = false;
        public long lastModifiedDate { get; set; } = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        public List<object> onlineAccounts { get; set; }
        public bool optIn { get; set; } = false;
        public bool optInLetter { get; set; } = false;
        public bool optInPhone { get; set; } = false;
        public bool optInSms { get; set; } = false;
        public string partyType { get; set; } = Configs.PartyType;
        public string customerType { get; set; } = Configs.CustomerType;
        public string primaryAddressId { get; set; } = "";
        public bool responsibleUserFixed { get; set; } = false;
        public string responsibleUserId { get; set; } = "";
        public string responsibleUserUsername { get; set; }
        public string salesChannel { get; set; } = Configs.SaleChannel;
        public List<object> tags { get; set; } 
        public bool useCustomsTariffNumber { get; set; } = false;
        public string vatRegistrationNumber { get; set; } = "";
        public string category { get; set; } = Configs.Standard;
        
    }

    public class WAddress
    {
        public string id { get; set; } = "";
        public string version { get; set; } = "";
        public string city { get; set; } = "";
        public string countryCode { get; set; } = "";
        public string country { get; set; } = "";
        public long createdDate { get; set; } = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        public bool deliveryAddress { get; set; } = false;
        public bool invoiceAddress { get; set; } = false;
        public long lastModifiedDate { get; set; } = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        public bool primeAddress { get; set; } = true;
        public string street1 { get; set; } = "";
        public string zipCode { get; set; } = "";
    }
}
