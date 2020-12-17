using BatchService.Model;
using Microsoft.Extensions.Logging;

namespace BatchService.Services
{
    public interface IProgressWriter
    {
        void WriteProgress<T>(Statistics statistics);
    }

    public class ProgressWriter : IProgressWriter
    {
        private readonly ILogger<ProgressWriter> _logger;

        public ProgressWriter(ILogger<ProgressWriter> logger)
        {
            _logger = logger;
        }

        public void WriteProgress<T>(Statistics stats)
        {
            var statistics = $@"Writing {typeof(T).Name}
Batch number: {stats.BatchNumber}/{stats.BatchesCount}
Batch progress: {stats.BatchProgress:#0.##%}
Total progress: {stats.TotalProgress:#0.##%}
Speed: {stats.Speed} items/s
End in: {stats.EndIn:c}";

            _logger.LogInformation(statistics);
        }
    }
}
