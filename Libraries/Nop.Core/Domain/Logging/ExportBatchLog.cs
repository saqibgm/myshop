using System;
using Nop.Core.Domain.Customers;

namespace Nop.Core.Domain.Logging
{
    /// <summary>
    /// Represents an activity log record
    /// </summary>
    public partial class ExportBatchLog : BaseEntity
    {
        public int ExportBatchId { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string ActionType { get; set; }
        public bool Success { get; set; }
        public string Details { get; set; }
        public DateTime CreatedOn { get; set; }

    }
}

