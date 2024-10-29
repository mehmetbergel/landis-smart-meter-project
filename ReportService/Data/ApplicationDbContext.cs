using Microsoft.EntityFrameworkCore;
using ReportService.Models.Database;

namespace ReportService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Report> Reports { get; set; }
        public ApplicationDbContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>().Property(p => p.UUID).HasDefaultValueSql("NEWSEQUENTIALID()");

        }
    }
}
