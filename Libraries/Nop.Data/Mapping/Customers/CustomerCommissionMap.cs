using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Customers;

namespace Nop.Data.Mapping.Customers
{
    /// <summary>
    /// Represents a customerCommission mapping configuration
    /// </summary>
    public partial class CustomerCommissionMap : NopEntityTypeConfiguration<CustomerCommission>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<CustomerCommission> builder)
        {
            builder.ToTable(nameof(CustomerCommission));
            builder.HasKey(customerCommission => customerCommission.Id);

            builder.Property(customerCommission => customerCommission.CommissionMinTargert);
            builder.Property(customerCommission => customerCommission.CommissionMonth);
            builder.Property(customerCommission => customerCommission.CommissionPercentage);
            builder.Property(customerCommission => customerCommission.BasicSalary);
            builder.Property(customerCommission => customerCommission.TotalCommission);
            builder.Property(customerCommission => customerCommission.TotalOrders);
            builder.Property(customerCommission => customerCommission.TotalProfit);
            builder.Property(customerCommission => customerCommission.TotalSalary);
            builder.Property(customerCommission => customerCommission.TotalSale);

            builder.Property(customerCommission => customerCommission.CustomerId).HasColumnName("Customer_Id");

            builder.HasOne(customerCommission => customerCommission.Customer)
                .WithMany()
                .HasForeignKey(customerCommission => customerCommission.CustomerId);

            base.Configure(builder);
        }

        #endregion
    }
}