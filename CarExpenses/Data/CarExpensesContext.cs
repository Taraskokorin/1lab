using Microsoft.EntityFrameworkCore;
using CarExpenses.Models;

namespace CarExpenses.Data;

public class CarExpensesContext : DbContext
{
    public CarExpensesContext(DbContextOptions<CarExpensesContext> options)
        : base(options) { }

    public DbSet<CarExpense> CarExpenses => Set<CarExpense>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // SQLite не має десяткового типу: decimal зберігаємо як double,
        // щоб сортування й Sum виконувала база (див. NOTES, ЛР 3).
        b.Entity<CarExpense>().Property(x => x.Amount).HasConversion<double>();
        b.Entity<CarExpense>().Property(x => x.Liters).HasConversion<double?>();

        // Одна й та сама витрата не вноситься двічі: день, пробіг і вид разом унікальні.
        b.Entity<CarExpense>()
            .HasIndex(x => new { x.SpentOn, x.Odometer, x.Kind })
            .IsUnique();
    }
}
