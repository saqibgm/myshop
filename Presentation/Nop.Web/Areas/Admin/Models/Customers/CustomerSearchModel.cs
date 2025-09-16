using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Core.Domain.Customers;

namespace Nop.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer search model
    /// </summary>
    public partial class CustomerSearchModel : BaseSearchModel, IAclSupportedModel
    {
        #region Ctor

        public CustomerSearchModel()
        {
            AvailableCountries = new List<SelectListItem>();
            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Customers.Customers.List.CustomerRoles")]
        public IList<int> SelectedCustomerRoleIds { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchId")]
        public string SearchId { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchEmail")]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchUsername")]
        public string SearchUsername { get; set; }

        public bool UsernamesEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchFirstName")]
        public string SearchFirstName { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchLastName")]
        public string SearchLastName { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchCity")]
        public string SearchCity { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchCountry")]
        public string SearchCountry { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchDateOfBirth")]
        public string SearchDayOfBirth { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchDateOfBirth")]
        public string SearchMonthOfBirth { get; set; }

        public bool DateOfBirthEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchCompany")]
        public string SearchCompany { get; set; }

        public bool CompanyEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchPhone")]
        public string SearchPhone { get; set; }

        public bool PhoneEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchZipCode")]
        public string SearchZipPostalCode { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchIpAddress")]
        public string SearchIpAddress { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.ItemsInCart")]
        public bool ItemsInCart { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchStatus")]
        public int SearchStatus { get; set; }
        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchSalesman")]
        public string SearchSalesman { get; set; }
        public bool AvatarEnabled { get; internal set; }

        public int AvatarSize { get; set; }
        public IList<SelectListItem> AvailableSalemanList { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }

        //

        #endregion
    }
}