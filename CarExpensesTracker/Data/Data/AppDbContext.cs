using Microsoft.EntityFrameworkCore;
using CarExpensesTracker.Models;

namespace CarExpensesTracker.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<FuelRecord> FuelRecords => Set<FuelRecord>();
        public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();
        public DbSet<OtherExpense> OtherExpenses => Set<OtherExpense>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CarExpensesTracker",
                "app.db");

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}