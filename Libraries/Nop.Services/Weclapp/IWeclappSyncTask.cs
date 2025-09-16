using System;
using System.Threading.Tasks;
using Nop.Services.Tasks;

namespace Nop.Services.Weclapp
{
    public partial interface IWeclappSyncTask
    {
        bool ExecuteProduct(bool retryFailed = false);
        bool ExecuteCustomer(bool retryFailed = false);
        bool ExecuteOrder(bool retryFailed = false);

    }
}