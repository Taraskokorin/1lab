using Microsoft.EntityFrameworkCore;
using CarExpenses.Models;

namespace CarExpenses.Services;

public record SummaryRow(string Name, int Count, decimal Total);

/// <summary>Витрата пального й вартість кілометра для одного автомобіля.</summary>
public record FuelRow(string Vehicle, decimal Liters, int Distance, decimal FuelCost, decimal TotalCost)
{
    /// <summary>Літрів на 100 км; null, якщо пробігу ще немає.</summary>
    public decimal? LitersPer100Km => Distance > 0 ? Liters * 100 / Distance : null;

    /// <summary>Скільки коштує кілометр з урахуванням усіх витрат.</summary>
    public decimal? CostPerKm => Distance > 0 ? TotalCost / Distance : null;
}

public static class SummaryCalculator
{
    // показник 1: групування за довідником (автомобілем)
    public static async Task<List<SummaryRow>> ByVehicleAsync(IQueryable<CarExpense> query) =>
        await query
            .GroupBy(x => x.Vehicle!.Name)
            .OrderByDescending(g => g.Sum(x => x.Amount))
            .Select(g => new SummaryRow(g.Key, g.Count(), g.Sum(x => x.Amount)))
            .ToListAsync();

    // показник 2: групування за періодом (місяцем)
    public static async Task<List<SummaryRow>> ByMonthAsync(IQueryable<CarExpense> query)
    {
        var raw = await query
            .GroupBy(x => new { x.SpentOn.Year, x.SpentOn.Month })
            .Select(g => new
            {
                g.Key.Year, g.Key.Month,
                Count = g.Count(),
                Total = g.Sum(x => x.Amount)
            })
            .ToListAsync();

        // Назву періоду складаємо вже в пам’яті: форматування рядка SQLite не перекладає.
        return raw
            .OrderBy(r => r.Year).ThenBy(r => r.Month)
            .Select(r => new SummaryRow($"{r.Month:D2}.{r.Year}", r.Count, r.Total))
            .ToList();
    }

    // показник 3: загальний підсумок, без групування
    public static async Task<SummaryRow> TotalAsync(IQueryable<CarExpense> query) =>
        new SummaryRow(
            "Усього",
            await query.CountAsync(),
            await query.SumAsync(x => x.Amount));

    // показник теми: витрата пального на 100 км і вартість кілометра
    public static async Task<List<FuelRow>> FuelByVehicleAsync(IQueryable<CarExpense> query)
    {
        // База рахує суми й межі пробігу; FuelRow складаємо вже в пам’яті,
        // бо конструктор record і впорядкування за ним SQLite не перекладає.
        var raw = await query
            .GroupBy(x => x.Vehicle!.Name)
            .Select(g => new
            {
                Vehicle = g.Key,
                Liters = g.Sum(x => x.Liters),
                MaxOdometer = g.Max(x => x.Odometer),
                MinOdometer = g.Min(x => x.Odometer),
                FuelCost = g.Where(x => x.Kind == ExpenseKind.Fuel).Sum(x => x.Amount),
                TotalCost = g.Sum(x => x.Amount)
            })
            .ToListAsync();

        return raw
            .OrderBy(r => r.Vehicle)
            .Select(r => new FuelRow(r.Vehicle, r.Liters ?? 0, r.MaxOdometer - r.MinOdometer,
                                     r.FuelCost, r.TotalCost))
            .ToList();
    }
}
