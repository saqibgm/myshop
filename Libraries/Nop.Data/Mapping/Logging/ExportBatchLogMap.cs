using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Nop.Data.Mapping.Customers
{
    /// <summary>
    /// Represents a ExportBatchLog mapping configuration
    /// </summary>
    public partial class ExportBatchLogMap : NopEntityTypeConfiguration<ExportBatchLog>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<ExportBatchLog> builder)
        {
            builder.ToTable(nameof(ExportBatchLog));
            builder.HasKey(ExportBatchLog => ExportBatchLog.Id);

            builder.Property(ExportBatchLog => ExportBatchLog.ExportBatchId);
            builder.Property(ExportBatchLog => ExportBatchLog.EntityType);
            builder.Property(ExportBatchLog => ExportBatchLog.EntityId);
            builder.Property(ExportBatchLog => ExportBatchLog.ActionType);
            builder.Property(ExportBatchLog => ExportBatchLog.Success);
            builder.Property(ExportBatchLog => ExportBatchLog.Details);
            builder.Property(ExportBatchLog => ExportBatchLog.CreatedOn);

            base.Configure(builder);
        }

        #endregion
    }
}