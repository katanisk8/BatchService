using System;
using BatchService.Model;

namespace BatchService.Services
{
    public interface IBenchmark
    {
        Statistics Calculate(BatchInfo batchInfo, decimal progress);
    }

    public class Benchmark : IBenchmark
    {
        public Statistics Calculate(BatchInfo batchInfo, decimal currentBatchProgress)
        {
            var batchNumber = CalculateBatchNumber(batchInfo.BatchIndex);
            var currentBatchInsertedRecords = CalculateCurrentBatchInsertedRecords(batchInfo.BatchSize, currentBatchProgress);
            var totalInsertedRecords = CalculateTotalInsertedRecords(batchInfo.BatchIndex, batchInfo.BatchSize, currentBatchInsertedRecords);
            var totalProgress = CalculateTotalProgress(totalInsertedRecords, batchInfo.RecordsCount);
            var totalSeconds = CalculateTotalSeconds(batchInfo.StartDateTime);
            var speed = CalculateSpeed(totalInsertedRecords, totalSeconds);
            var totalSecondsForecast = CalculateTotalSecondsForecast(batchInfo.RecordsCount, speed);
            var endSecondsForecast = CalculateEndSecondsForecast(totalSecondsForecast, totalSeconds);
            var timeSpanForecast = GetTimeSpan(endSecondsForecast);

            return new Statistics
            {
                BatchNumber = batchNumber,
                BatchesCount = batchInfo.BatchesCount,
                BatchProgress = currentBatchProgress,
                TotalProgress = totalProgress,
                Speed = (int)speed,
                EndIn = timeSpanForecast
            };
        }

        private static long CalculateBatchNumber(in long batchIndex) => batchIndex + 1;

        private static decimal CalculateTotalInsertedRecords(long batchIndex, long batchSize, decimal currentBatchInsertedRecords) =>
            batchIndex * batchSize + currentBatchInsertedRecords;

        private static decimal CalculateCurrentBatchInsertedRecords(in long batchSize, decimal currentBatchProgress) =>
            batchSize * currentBatchProgress;

        private static decimal CalculateTotalProgress(in decimal totalInsertedRecords, in long totalRecordsCount) =>
            totalInsertedRecords / totalRecordsCount;

        private static decimal CalculateTotalSeconds(in DateTime startDateTime) =>
            (decimal)(DateTime.Now - startDateTime).TotalSeconds;

        private static decimal CalculateSpeed(in decimal totalInsertedRecords, in decimal totalSeconds) =>
            (totalInsertedRecords / totalSeconds);

        private static decimal CalculateTotalSecondsForecast(in long totalRecordsCount, in decimal speed) =>
            totalRecordsCount / speed;

        private static int CalculateEndSecondsForecast(in decimal totalSecondsForecast, in decimal totalSeconds) =>
            (int)(totalSecondsForecast - totalSeconds);

        private static TimeSpan GetTimeSpan(in int endSecondsForecast) =>
            new TimeSpan(0, 0, 0, endSecondsForecast);
    }
}
