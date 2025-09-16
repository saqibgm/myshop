namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents an "order by country" report line
    /// </summary>
    public partial class OrderByCountryReportLine
    {
        /// <summary>
        /// Country identifier; null for unknown country
        /// </summary>
        public int? CountryId { get; set; }

        /// <summary>
        /// Gets or sets the number of orders
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// Gets or sets the order total summary
        /// </summary>
        public decimal SumOrders { get; set; }
    }
    public partial class OrderByCommissionModelReportLine
    {
        /// <summary>
        /// Country identifier; null for unknown country
        /// </summary>
        public int? SalemanId { get; set; } 
        /// <summary>
        /// Gets or sets the order total summary
        /// </summary>
        public decimal TotalSale { get; set; }
        public decimal TotalProfit { get; set; }
        /// <summary>
        /// Gets or sets the number of orders
        /// </summary>
        public int TotalOrders { get; set; }
        /// <summary>
        /// Gets or sets the number of orders
        /// </summary>
        public decimal CommissionAmount { get; set; }
        /// <summary>
        /// Gets or sets the number of orders
        /// </summary>       
        public decimal CommissionPercentage { get; set; }
        /// <summary>
        /// Gets or sets the TotalSalemanSalaryIncCommission
        /// </summary>
        public decimal TotalSalemanSalaryIncCommission { get; set; }
        /// <summary>
        /// BasicSalary identifier; null for unknown BasicSalary
        /// </summary>
        public decimal BasicSalary { get; set; }

      
    }
    public partial class OrderBySalemanStatisticsReportLine
    {
        public int? UniqueCustomers { get; set; }

        public decimal TotalSale { get; set; }

        public int? Order { get; set; }

        public decimal Commission { get; set; }

        public decimal TotalProfit { get; set; }

        public decimal UnpaidAmount { get; set; }

        public string MonthName { get; set; }
    }
    public partial class OrderByCustomerProfitReportLine
    {
        public string CustomerName { get; set; }
        public int CustomerId { get; set; }

        public int InvoiceCount { get; set; }

        public decimal TotalSale { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Profit { get; set; }

        public decimal ProfitTotal { get; set; }

        public decimal CommissionAmmount { get; set; }
    

        public string TotalCommission { get; set; }
    }
}
