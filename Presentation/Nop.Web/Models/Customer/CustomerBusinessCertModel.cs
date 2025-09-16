using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Customer
{
    public partial class CustomerBusinessCertModel : BaseNopModel
    {
        public int CustomerId { get; set; }
        public int BusinessCertId { get; set; }
        public string BusinessCertUrl { get; set; }
        public bool IsCustomer { get; set; }

    }
}