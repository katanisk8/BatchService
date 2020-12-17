using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BatchService.Database;
using BatchService.Entities;
using BatchService.Model;
using Microsoft.Extensions.Logging;

namespace BatchService.Services
{
    public interface IInitializer
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }

    public class Initializer : IInitializer
    {
        private const long StudentsCount = 1_000_000;
        private const int BatchSize = 100_000;
        private const int BatchesCount = (int)StudentsCount / BatchSize;
        private readonly bool WithRelation = true;
        private readonly InsertMethod _insertMethod = InsertMethod.BulkInsert;

        private readonly ILogger<Initializer> _logger;
        private readonly IBulkInsert _bulkInsert;
        private readonly IWriteToServer _writeToServer;
        private readonly IMeasurements _measurements;

        public Initializer(
            ILogger<Initializer> logger,
            IBulkInsert bulkInsert,
            IWriteToServer writeToServer,
            IMeasurements measurements)
        {
            _logger = logger;
            _bulkInsert = bulkInsert;
            _writeToServer = writeToServer;
            _measurements = measurements;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting initialization...");

                var insertMethod = GetInsertMethod();
                var insertTask = InsertAsync(insertMethod, cancellationToken);

                var elapsed = await _measurements.MeasureAsync(insertTask);

                LogStatistics(elapsed);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Initialization was cancelled!");
            }
        }

        private Func<List<Student>, BatchInfo, CancellationToken, Task> GetInsertMethod() =>
            _insertMethod switch
            {
                // With relation - 
                // Without relation - Finished initialization in 00:00:19.6984067 - 52,631 [items/s]
                InsertMethod.BulkInsert => BulkInsertAsync,


                // With relation - Finished initialization in 00:00:29.1834154 - 34,482 [items/s]
                // Without relation - Finished initialization in 00:00:11.5048637 - 90,909 [items/s]
                InsertMethod.WriteToServer => WriteToServerAsync,
                _ => throw new ArgumentOutOfRangeException(nameof(_insertMethod), _insertMethod, null)
            };

        private async Task BulkInsertAsync(List<Student> students, BatchInfo batchInfo, CancellationToken cancellationToken)
        {
            var grades = students.Select(x => x.Grade).ToList();
            await _bulkInsert.BulkInsertAsync(grades, batchInfo, cancellationToken);

            if (WithRelation)
            {
                students.ForEach(x => x.GradeId = x.Grade.Id);
                await _bulkInsert.BulkInsertAsync(students, batchInfo, cancellationToken);
            }
        }

        private async Task WriteToServerAsync(List<Student> students, BatchInfo batchInfo, CancellationToken cancellationToken)
        {
            var grades = students.Select(x => x.Grade).ToList();
            await _writeToServer.WriteToServerAsync(grades, batchInfo, cancellationToken);

            if (WithRelation)
            {
                students.ForEach(x => x.GradeId = x.Grade.Id);
                await _writeToServer.WriteToServerAsync(students, batchInfo, cancellationToken);
            }
        }

        private static async Task InsertAsync(Func<List<Student>, BatchInfo, CancellationToken, Task> insertMethod, CancellationToken cancellationToken)
        {
            var batchInfo = GetBatchInfo();

            for (var batchIndex = 0; batchIndex < BatchesCount; batchIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                batchInfo.BatchIndex = batchIndex;
                var students = GetStudents(batchIndex, cancellationToken);

                await insertMethod(students, batchInfo, cancellationToken);
            }
        }

        private static BatchInfo GetBatchInfo() =>
            new BatchInfo
            {
                RecordsCount = StudentsCount,
                BatchSize = BatchSize,
                BatchesCount = BatchesCount,
                StartDateTime = DateTime.Now
            };

        private static List<Student> GetStudents(long batchIndex, CancellationToken cancellationToken) =>
            Enumerable
                .Range(1, (int)BatchSize)
                .Select((x, i) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var index = batchIndex * BatchSize + x;

                    return GetStudent(index);
                })
                .ToList();

        private static Student GetStudent(long i) =>
            new Student
            {
                Id = i,
                CreateDateTime = DateTime.Now,
                ModifyDateTime = DateTime.Now,
                StudentName = $"{nameof(Student.StudentName)}_{i}",
                DateOfBirth = DateTime.Now.AddYears(-20),
                CityOfBirth = $"{nameof(Student.CityOfBirth)}_{i}",
                Height = i,
                Weight = i,
                Firstname = $"{nameof(Student.Firstname)}_{i}",
                Surname = $"{nameof(Student.Surname)}_{i}",
                Grade = new Grade
                {
                    Id = i,
                    CreateDateTime = DateTime.Now,
                    ModifyDateTime = DateTime.Now,
                    Name = $"{nameof(Grade.Name)}_{i}",
                    Section = $"{nameof(Grade.Section)}_{i}"
                }
            };

        private void LogStatistics(in TimeSpan elapsed)
        {
            var speed = StudentsCount / elapsed.Seconds;

            _logger.LogInformation($"Finished initialization in {elapsed:c} - {speed:#,##0} [items/s]");
        }
    }
}
