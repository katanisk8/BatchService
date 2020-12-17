using System;

namespace BatchService.Model
{
    public class BatchInfo
    {
        public long RecordsCount { get; set; }
        public int BatchSize { get; set; }
        public int BatchesCount { get; set; }
        public int BatchIndex { get; set; }
        public DateTime StartDateTime { get; set; }
    }
}
