using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Services.Weclapp.model;

namespace Nop.Services.Logging
{
    /// <summary>
    /// Customer activity service
    /// </summary>
    public class ExportLogService : IExportLogService
    {
        #region Fields

        private readonly IDbContext _dbContext;
        private readonly IRepository<ExportBatch> _exportBatchRepository;
        private readonly IRepository<ExportBatchLog> _exportBatchLogRepository;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ExportLogService(IDbContext dbContext,
            IRepository<ExportBatch> exportBatchRepository,
            IRepository<ExportBatchLog> exportBatchLogRepository,
            IStaticCacheManager cacheManager,
            IWebHelper webHelper,
            IWorkContext workContext)
        {
            _dbContext = dbContext;
            _exportBatchRepository = exportBatchRepository;
            _exportBatchLogRepository = exportBatchLogRepository;
            _cacheManager = cacheManager;
            _webHelper = webHelper;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public virtual ExportBatch GetLastBatch(string entityType)
        {
            var lastBatch = _exportBatchRepository.Table.Where(b=>b.EntityType== entityType).OrderByDescending(e=>e.Id).FirstOrDefault();
            return lastBatch;
        }
        public virtual void InsertExportBatch(ExportBatch exportBatch)
        {
            if (exportBatch == null)
                throw new ArgumentNullException(nameof(exportBatch));
            exportBatch.ExportedOn = DateTime.Now;
            _exportBatchRepository.Insert(exportBatch);
        }

        public virtual void UpdateExportBatch(ExportBatch exportBatch)
        {
            if (exportBatch == null)
                throw new ArgumentNullException(nameof(exportBatch));
            _exportBatchRepository.Update(exportBatch);
        }

        public virtual void DeleteExportBatch(ExportBatch exportBatch)
        {
            if (exportBatch == null)
                throw new ArgumentNullException(nameof(exportBatch));

            _exportBatchRepository.Delete(exportBatch);
            _cacheManager.RemoveByPrefix(NopLoggingDefaults.ActivityTypePrefixCacheKey);
        }

        public virtual void InsertExportBatchLog(ExportBatchLog exportBatchLog)
        {
            if (exportBatchLog == null)
                throw new ArgumentNullException(nameof(exportBatchLog));
            exportBatchLog.CreatedOn = DateTime.Now;
            _exportBatchLogRepository.Insert(exportBatchLog);
        }

        public virtual void UpdateExportBatchLog(ExportBatchLog exportBatchLog)
        {
            if (exportBatchLog == null)
                throw new ArgumentNullException(nameof(exportBatchLog));

            _exportBatchLogRepository.Update(exportBatchLog);
        }

        public virtual void DeleteExportBatchLog(ExportBatchLog exportBatchLog)
        {
            if (exportBatchLog == null)
                throw new ArgumentNullException(nameof(exportBatchLog));

            _exportBatchLogRepository.Delete(exportBatchLog);
            _cacheManager.RemoveByPrefix(NopLoggingDefaults.ActivityTypePrefixCacheKey);
        }



        #endregion
    }
}