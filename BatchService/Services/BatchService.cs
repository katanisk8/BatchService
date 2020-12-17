using System;
using System.Collections.Generic;
using System.Threading;
using BatchService.Database;

namespace BatchService.Services
{
    public interface IBatchService
    {
        IAsyncEnumerable<List<T>> BatchAsync<T>(BatchDbContext context, CancellationToken cancellationToken);
    }

    public class BatchService : IBatchService
    {
        public IAsyncEnumerable<List<T>> BatchAsync<T>(BatchDbContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
