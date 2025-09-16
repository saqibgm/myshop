using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Services.Weclapp.model;

namespace Nop.Services.Logging
{
    /// <summary>
    /// Customer activity service interface
    /// </summary>
    public partial interface IExportLogService
    {
        void InsertExportBatch(ExportBatch exportBatch);
        ExportBatch GetLastBatch(string entityTypes);

        void UpdateExportBatch(ExportBatch exportBatch);
                
        void DeleteExportBatch(ExportBatch exportBatch);
        
        void InsertExportBatchLog(ExportBatchLog exportBatchLog);

        void UpdateExportBatchLog(ExportBatchLog exportBatchLog);

        void DeleteExportBatchLog(ExportBatchLog exportBatchLog);


    }
}
