using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Nop.Data.Mapping.Customers
{
    /// <summary>
    /// Represents a ExportBatch mapping configuration
    /// </summary>
    public partial class ExportBatchMap : NopEntityTypeConfiguration<ExportBatch>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<ExportBatch> builder)
        {
            builder.ToTable(nameof(ExportBatch));
            builder.HasKey(ExportBatch => ExportBatch.Id);

            builder.Property(ExportBatch => ExportBatch.EntityType);
            builder.Property(ExportBatch => ExportBatch.Failed);
            builder.Property(ExportBatch => ExportBatch.Inserted);
            builder.Property(ExportBatch => ExportBatch.Updated);
            builder.Property(ExportBatch => ExportBatch.Completed);
            builder.Property(ExportBatch => ExportBatch.ExportedOn);

            base.Configure(builder);
        }

        #endregion
    }
}