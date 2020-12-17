using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BatchService.Model;
using BatchService.Services;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace BatchService.Database
{
    public interface IBulkInsert
    {
        Task BulkInsertAsync<T>(IList<T> entities, BatchInfo batchInfo, CancellationToken cancellationToken) where T : class;
    }

    public class BulkInsert : IBulkInsert
    {
        private readonly IDbContextFactory<BatchDbContext> _contextFactory;
        private readonly IBenchmark _benchmark;
        private readonly IProgressWriter _progressWriter;

        public BulkInsert(
            IDbContextFactory<BatchDbContext> contextFactory,
            IBenchmark benchmark,
            IProgressWriter progressWriter)
        {
            _contextFactory = contextFactory;
            _benchmark = benchmark;
            _progressWriter = progressWriter;
        }

        public async Task BulkInsertAsync<T>(IList<T> entities, BatchInfo batchInfo, CancellationToken cancellationToken) where T : class
        {
            await using var context = _contextFactory.CreateDbContext();

            await context.BulkInsertAsync(
                entities,
                config =>
                {
                    //config.BatchSize = batchInfo.BatchSize;
                    config.BulkCopyTimeout = 0;
                },
                progress => Progress<T>(batchInfo, progress),
                cancellationToken);
        }

        private void Progress<T>(BatchInfo batchInfo, in decimal progress)
        {
            var stats = _benchmark.Calculate(batchInfo, progress);
            _progressWriter.WriteProgress<T>(stats);
        }
    }
}
