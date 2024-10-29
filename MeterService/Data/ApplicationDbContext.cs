using MeterService.Models;
using Microsoft.EntityFrameworkCore;

namespace MeterService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<MeterReading> MeterReadings { get; set; }
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeterReading>().Property(p => p.UUID).HasDefaultValueSql("NEWSEQUENTIALID()");
        }
    }

}