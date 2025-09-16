using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer
    /// </summary>
    public partial class CustomerCommission : BaseEntity
    {

        public CustomerCommission()
        {
        }

        public int CustomerId { get; set; }
        public DateTime CommissionMonth { get; set; }
        public decimal? BasicSalary { get; set; }
        public decimal? CommissionPercentage { get; set; }
        public decimal? CommissionMinTargert { get; set; }
        public int TotalOrders{ get; set; }
        public decimal TotalSale { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal TotalSalary { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime LastActivityDateUtc { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

        public virtual Customer Customer { get; set; }

        #region Navigation properties

        #endregion

    }
}