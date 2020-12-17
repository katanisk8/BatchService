using System;

namespace BatchService.Model
{
    public class Statistics
    {
        public long BatchNumber { get; set; }
        public long BatchesCount { get; set; }
        public decimal BatchProgress { get; set; }
        public decimal TotalProgress { get; set; }
        public long Speed { get; set; }
        public TimeSpan EndIn { get; set; }
    }
}
