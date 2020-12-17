using System.Linq;
using BatchService.Entities;
using Microsoft.EntityFrameworkCore;

namespace BatchService.Database
{
    public class BatchDbContext : DbContext
    {
        public DbSet<Student> Student { get; set; }
        public DbSet<Grade> Grade { get; set; }
        public DbSet<Batching> Batching { get; set; }

        public BatchDbContext(DbContextOptions<BatchDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?))
                .ToList()
                .ForEach(x => x.SetColumnType("decimal(18, 2)"));

            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.Grade).WithMany(x => x.Students).HasForeignKey(x => x.GradeId);
            });

            modelBuilder.Entity<Grade>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasMany(x => x.Students).WithOne(x => x.Grade).HasForeignKey(x => x.GradeId);
            });

            modelBuilder.Entity<Batching>(entity =>
            {
                entity.HasKey(x => x.StudentId);
            });
        }
    }
}
