using Nop.Services.Catalog;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Nop.Services.Weclapp.model;
using Nop.Core.Domain.Catalog;
namespace Nop.Services.Weclapp
{
    public partial interface IWeclappDataService
    {
        IList<Product> GetNopProducts();
        List<WCustomer> GetNopCustomers();
        List<WOrder> GetNopOrders();

    }

}