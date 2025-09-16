using System;
using Nop.Core.Domain.Customers;

namespace Nop.Core.Domain.Logging
{
    /// <summary>
    /// Represents an activity log record
    /// </summary>
    public partial class ExportBatch : BaseEntity
    {
        public string EntityType { get; set; }
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Failed { get; set; }
        public bool Completed { get; set; }

        public DateTime ExportedOn { get; set; }

    }
}

