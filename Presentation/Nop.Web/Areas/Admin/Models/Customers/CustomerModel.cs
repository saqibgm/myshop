using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nop.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer model
    /// </summary>
    public partial class CustomerModel : BaseNopEntityModel, IAclSupportedModel
    {
        #region Ctor

        public CustomerModel()
        {
            AvailableTimeZones = new List<SelectListItem>();
            SendEmail = new SendEmailModel() { SendImmediately = true };
            SendPm = new SendPmModel();

            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();
            AvailableManagers = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
            AvailableSalemanList = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            CustomerAttributes = new List<CustomerAttributeModel>();
            AvailableNewsletterSubscriptionStores = new List<SelectListItem>();
            SelectedNewsletterSubscriptionStoreIds = new List<int>();
            AddRewardPoints = new AddRewardPointsToCustomerModel();
            CustomerRewardPointsSearchModel = new CustomerRewardPointsSearchModel();
            CustomerAddressSearchModel = new CustomerAddressSearchModel();
            CustomerOrderSearchModel = new CustomerOrderSearchModel();
            CustomerCommissionSearchModel = new CustomerCommissionSearchModel();
            CustomerShoppingCartSearchModel = new CustomerShoppingCartSearchModel();
            CustomerActivityLogSearchModel = new CustomerActivityLogSearchModel();
            CustomerBackInStockSubscriptionSearchModel = new CustomerBackInStockSubscriptionSearchModel();
            CustomerAssociatedExternalAuthRecordsSearchModel = new CustomerAssociatedExternalAuthRecordsSearchModel();
        }

        #endregion

        #region Properties

        public bool UsernamesEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Username")]
        public string Username { get; set; }

        //public bool RemarksEnabled { get; set; }

        //[NopResourceDisplayName("Admin.Customers.Customers.Fields.Remarks")]
        //public string Remarks { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Password")]
        [DataType(DataType.Password)]
        [NoTrim]
        public string Password { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Vendor")]
        public int VendorId { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }
        //form fields & properties
        public bool GenderEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Gender")]
        public string Gender { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.LastName")]
        public string LastName { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.FullName")]
        public string FullName { get; set; }
        [NopResourceDisplayName("admin.customers.customer.fields.Manager")]
        public int ReportingTo { get; set; }
        [NopResourceDisplayName("admin.customers.customer.fields.TeamMemberOfSaleman")]
        public int TeamMemberOfSaleman { get; set; }


        public bool DateOfBirthEnabled { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }
        public bool DateInvoiceEnabled { get; set; } = true;
        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.DateInvoice")]
        public DateTime? DateInvoice { get; set; }

        public bool CompanyEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Company")]
        public string Company { get; set; }

        public bool BasicSalaryEnabled { get; set; } = true;
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.BasicSalary")]
        //[NopResourceDisplayName("Basic Salary")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? BasicSalary { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.CommissionPercentage")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CommissionPercentage { get; set; } = 0;
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.CommissionMinTargert")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CommissionMinTargert { get; set; } = 0;

        public bool RemarksEnabled { get; set; } = true;
        //[NopResourceDisplayName("Admin.Customers.Customers.Fields.Remarks")]
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Remarks")]
        public string Remarks { get; set; }
        public bool NetDiscountEnabled { get; set; } = true;
        // [NopResourceDisplayName("Admin.Customers.Customers.Fields.NetDiscount")]
        [NopResourceDisplayName("admin.orders.quickorder.fields.NetDiscount")]
        public string NetDiscount { get; set; }
        public bool BillAmoutEnabled { get; set; } = true;
        // [NopResourceDisplayName("Admin.Customers.Customers.Fields.BillAmount")]
        [NopResourceDisplayName("admin.orders.quickorder.fields.BillAmount")]
        public string BillAmount { get; set; }
        public bool PaidAmountEnabled { get; set; } = true;
        //[NopResourceDisplayName("Admin.Customers.Customers.Fields.PaidAmount")]
        [NopResourceDisplayName("admin.orders.quickorder.fields.PaidAmount")]
        public string PaidAmount { get; set; }
        public bool DueBalanceEnabled { get; set; } = true;
        //[NopResourceDisplayName("Admin.Customers.Customers.Fields.DueBalance")]
        [NopResourceDisplayName("admin.orders.quickorder.fields.DueBalance")]
        public string DueBalance { get; set; }

        public bool StreetAddressEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.StreetAddress")]
        public string StreetAddress { get; set; }

        public bool StreetAddress2Enabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.StreetAddress2")]
        public string StreetAddress2 { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        public bool CityEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.City")]
        public string City { get; set; }

        public bool CountyEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.County")]
        public string County { get; set; }

        public bool CountryEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Country")]
        public int CountryId { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }

        public IList<SelectListItem> AvailableManagers { get; set; }
        public IList<SelectListItem> AvailableSalemanList { get; set; }

        public bool StateProvinceEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.StateProvince")]
        public int StateProvinceId { get; set; }

        public IList<SelectListItem> AvailableStates { get; set; }

        public bool PhoneEnabled { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Phone")]
        public string Phone { get; set; }

        public bool FaxEnabled { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Fax")]
        public string Fax { get; set; }

        public List<CustomerAttributeModel> CustomerAttributes { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.RegisteredInStore")]
        public string RegisteredInStore { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.AdminComment")]
        public string AdminComment { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.IsTaxExempt")]
        public bool IsTaxExempt { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Affiliate")]
        public int AffiliateId { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Affiliate")]
        public string AffiliateName { get; set; }

        //time zone
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.TimeZoneId")]
        public string TimeZoneId { get; set; }

        public bool AllowCustomersToSetTimeZone { get; set; }

        public IList<SelectListItem> AvailableTimeZones { get; set; }

        //EU VAT
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.VatNumber")]
        public string VatNumber { get; set; }

        public string VatNumberStatusNote { get; set; }

        public bool DisplayVatNumber { get; set; }

        //registration date
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.LastActivityDate")]
        public DateTime LastActivityDate { get; set; }

        //IP address
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.IPAddress")]
        public string LastIpAddress { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.LastVisitedPage")]
        public string LastVisitedPage { get; set; }

        //customer roles
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.CustomerRoles")]
        public string CustomerRoleNames { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.CustomerRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }
        //price Group
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.CustomerRoles")]
        // public IList<SelectListItem> AvailablePriceGroup { get; set; }
        //[NopResourceDisplayName("Admin.Orders.Fields.PriceGroup")]
        public IList<int> PriceGroup { get; set; }


        public bool IsLoggedInAsVendor { get; set; }
        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        [NopResourceDisplayName("Admin.Orders.List.OrderStatus")]
        public IList<int> OrderStatusIds { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.CustomerRoles")]
        public IList<SelectListItem> AvailableNewsletterSubscriptionStores { get; set; }
        //public IList<int> SelectedPriceGroupIds { get; set; }
        //newsletter subscriptions (per store)
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Newsletter")]

        //public IList<SelectListItem> AvailablePriceGroup { get; set; }
        //  [NopResourceDisplayName("Admin.Customers.Customers.Fields.Newsletter")]
        public IList<int> SelectedNewsletterSubscriptionStoreIds { get; set; }
        //...........New Role..........
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.CustomerRoles")]
        public string PackingType { get; set; }
        public IList<SelectListItem> AvailablePackingType { get; set; }
        [NopResourceDisplayName("Admin.Orders.List.Admin.Orders.List.PackingType")]
        public IList<int> SelectedPackingTypeIds { get; set; }

        //..............role end.........

        //reward points history
        public bool DisplayRewardPointsHistory { get; set; }

        public AddRewardPointsToCustomerModel AddRewardPoints { get; set; }

        public CustomerRewardPointsSearchModel CustomerRewardPointsSearchModel { get; set; }

        //send email model
        public SendEmailModel SendEmail { get; set; }

        //send PM model
        public SendPmModel SendPm { get; set; }

        //send the welcome message
        public bool AllowSendingOfWelcomeMessage { get; set; }

        //re-send the activation message
        public bool AllowReSendingOfActivationMessage { get; set; }


        //GDPR enabled
        public bool DateOfInvoiceEnabled { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.DateOfInvoice")]
        public bool DateOfInvoice { get; set; }
        public bool GdprEnabled { get; set; }

        public string AvatarUrl { get; internal set; }
        public string BusinessCertUrl { get; internal set; }
        public int BusinessCertId { get; set; }
        public bool IsCustomer { get; internal set; }
        public bool IsSalesman { get; internal set; }
        public bool IsAdmin { get; internal set; }
        public bool IsManager { get; internal set; }
        public bool IsRegistered { get; internal set; }
        [NopResourceDisplayName("Admin.Customers.Customers.Fields.IsExportAllowed")]
        public bool IsExportAllowed { get; set; }
        public CustomerAddressSearchModel CustomerAddressSearchModel { get; set; }

        public CustomerOrderSearchModel CustomerOrderSearchModel { get; set; }
        public CustomerCommissionSearchModel CustomerCommissionSearchModel { get; set; }

        public CustomerShoppingCartSearchModel CustomerShoppingCartSearchModel { get; set; }

        public CustomerActivityLogSearchModel CustomerActivityLogSearchModel { get; set; }

        public CustomerBackInStockSubscriptionSearchModel CustomerBackInStockSubscriptionSearchModel { get; set; }

        public CustomerAssociatedExternalAuthRecordsSearchModel CustomerAssociatedExternalAuthRecordsSearchModel { get; set; }



        #endregion

        #region Nested classes

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

        public partial class SendPmModel : BaseNopModel
        {
            [NopResourceDisplayName("Admin.Customers.Customers.SendPM.Subject")]
            public string Subject { get; set; }

            [NopResourceDisplayName("Admin.Customers.Customers.SendPM.Message")]
            public string Message { get; set; }
        }

        public partial class CustomerAttributeModel : BaseNopEntityModel
        {
            public CustomerAttributeModel()
            {
                Values = new List<CustomerAttributeValueModel>();
            }

            public string Name { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            /// Default value for textboxes
            /// </summary>
            public string DefaultValue { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<CustomerAttributeValueModel> Values { get; set; }
        }

        public partial class CustomerAttributeValueModel : BaseNopEntityModel
        {
            public string Name { get; set; }

            public bool IsPreSelected { get; set; }
        }
        public partial class CustomerSalemanListItem : SelectListItem
        {
            public int SalemanId { get; set; }

        }
        #endregion
    }
}