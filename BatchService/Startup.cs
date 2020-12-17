using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BatchService.Database;
using BatchService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BatchService
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMvcCore().AddApiExplorer();

            var connectionString = _configuration.GetConnectionString("BatchConnection");

            void Options(DbContextOptionsBuilder x) =>
                x.UseSqlServer(connectionString,
                    opt => opt.CommandTimeout((int)TimeSpan.FromHours(1).TotalSeconds));

            services.AddDbContextPool<BatchDbContext>(Options);
            services.AddPooledDbContextFactory<BatchDbContext>(Options);

            services.AddScoped<IBulkInsert, BulkInsert>();
            services.AddScoped<IWriteToServer, WriteToServer>();
            services.AddScoped<IInitializer, Initializer>();
            services.AddScoped<IBenchmark, Benchmark>();
            services.AddScoped<IProgressWriter, ProgressWriter>();
            services.AddScoped<IBatchService, Services.BatchService>();
            services.AddScoped<IMeasurements, Measurements>();

            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}
