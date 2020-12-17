using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BatchService.Model;
using BatchService.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BatchService.Database
{
    public interface IWriteToServer
    {
        Task WriteToServerAsync<T>(IEnumerable<T> entities, BatchInfo batchInfo, CancellationToken cancellationToken);
    }

    public class WriteToServer : IWriteToServer
    {
        private readonly IDbContextFactory<BatchDbContext> _contextFactory;
        private readonly IBenchmark _benchmark;
        private readonly IProgressWriter _progressWriter;

        public WriteToServer(
            IDbContextFactory<BatchDbContext> contextFactory,
            IBenchmark benchmark,
            IProgressWriter progressWriter)
        {
            _contextFactory = contextFactory;
            _benchmark = benchmark;
            _progressWriter = progressWriter;
        }

        public async Task WriteToServerAsync<T>(IEnumerable<T> entities, BatchInfo batchInfo, CancellationToken cancellationToken)
        {
            await using var context = _contextFactory.CreateDbContext();

            await using var connection = new SqlConnection(context.Database.GetConnectionString());
            await connection.OpenAsync(cancellationToken);

            var bulkCopy = GetSqlBulkCopy<T>(connection, batchInfo);

            bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler((sender, e) => OnSqlRowsCopied<T>(sender, e, batchInfo));

            var table = GetDataTable(entities);

            await bulkCopy.WriteToServerAsync(table, cancellationToken);
        }

        private static SqlBulkCopy GetSqlBulkCopy<T>(SqlConnection connection, BatchInfo batchInfo)
        {
            var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = typeof(T).Name,
                BulkCopyTimeout = 0,
                NotifyAfter = 2000,
                //BatchSize = batchInfo.BatchSize
            };

            return bulkCopy;
        }

        private static DataTable GetDataTable<T>(IEnumerable<T> entities)
        {
            var properties = GetProperties<T>();
            var dataTable = new DataTable();
            AddColumns(dataTable, properties);
            AddRows(dataTable, properties, entities);

            return dataTable;
        }

        private static List<PropertyInfo> GetProperties<T>() => typeof(T)
            .GetProperties()
            .Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string))
            .ToList();

        private static void AddColumns(DataTable dataTable, IEnumerable<PropertyInfo> properties)
        {
            var columns = properties
                .Select(x =>
                {
                    var propertyType = x.PropertyType;
                    if (x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        propertyType = Nullable.GetUnderlyingType(x.PropertyType) ?? x.PropertyType;
                    }

                    return new DataColumn(x.Name, propertyType);
                })
                .ToArray();

            dataTable.Columns.AddRange(columns);
        }

        private static void AddRows<T>(DataTable dataTable, IReadOnlyCollection<PropertyInfo> properties, IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                var values = properties.Select(x => x.GetValue(entity, null) ?? DBNull.Value).ToArray();

                dataTable.Rows.Add(values);
            }
        }

        private void OnSqlRowsCopied<T>(object sender, SqlRowsCopiedEventArgs e, BatchInfo batchInfo)
        {
            var progress = (decimal)e.RowsCopied / batchInfo.BatchSize;
            Progress<T>(batchInfo, progress);
        }

        private void Progress<T>(BatchInfo batchInfo, in decimal progress)
        {
            var stats = _benchmark.Calculate(batchInfo, progress);
            _progressWriter.WriteProgress<T>(stats);
        }
    }
}
