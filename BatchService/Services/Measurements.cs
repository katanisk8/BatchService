using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BatchService.Services
{
    public interface IMeasurements
    {
        Task<TimeSpan> MeasureAsync(Task task);
    }

    public class Measurements : IMeasurements
    {
        public async Task<TimeSpan> MeasureAsync(Task task)
        {
            var sw = Stopwatch.StartNew();

            await task;

            sw.Stop();

            return sw.Elapsed;
        }
    }
}
