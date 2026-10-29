using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using CarExpenses.Data;
using CarExpenses.Models;
using CarExpenses.Services;

namespace CarExpenses.Tests;

public class SummaryCalculatorTests : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly CarExpensesContext db;
    private readonly Vehicle skoda = new() { Name = "Skoda Octavia" };
    private readonly Vehicle logan = new() { Name = "Renault Logan" };
    private readonly Place station = new() { Name = "АЗС ОККО" };

    public SummaryCalculatorTests()
    {
        // База в оперативній пам’яті живе, доки відкрите з’єднання.
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<CarExpensesContext>()
            .UseSqlite(connection)
            .Options;

        db = new CarExpensesContext(options);
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }

    // Запис з усіма обов’язковими полями та довідниками – щоб тест не падав
    // на зовнішньому ключі чи унікальному індексі, а перевіряв саме обчислення.
    private CarExpense Expense(Vehicle vehicle, decimal amount, DateOnly date, int odometer,
                               ExpenseKind kind = ExpenseKind.Fuel, decimal? liters = null) =>
        new()
        {
            Title = "Витрата", TitleSearch = "витрата", Kind = kind, Amount = amount,
            SpentOn = date, Odometer = odometer, Liters = liters,
            Vehicle = vehicle, Place = station
        };

    [Fact]
    public async Task ByVehicleForEmptySetReturnsNoRows()
    {
        var result = await SummaryCalculator.ByVehicleAsync(db.CarExpenses);

        Assert.Empty(result);
    }

    [Fact]
    public async Task TotalForEmptySetIsZero()
    {
        var total = await SummaryCalculator.TotalAsync(db.CarExpenses);

        Assert.Equal(0, total.Count);
        Assert.Equal(0m, total.Total);
    }

    [Fact]
    public async Task ExpensesOfOneVehicleMergeIntoSingleRow()
    {
        db.CarExpenses.AddRange(
            Expense(skoda, 1450m, new DateOnly(2026, 9, 2), 84210),
            Expense(skoda, 1520m, new DateOnly(2026, 9, 11), 84690));
        await db.SaveChangesAsync();

        var result = await SummaryCalculator.ByVehicleAsync(db.CarExpenses);

        var row = Assert.Single(result);
        Assert.Equal("Skoda Octavia", row.Name);
        Assert.Equal(2, row.Count);
    }

    [Fact]
    public async Task TotalEqualsSumOfAmounts()
    {
        db.CarExpenses.AddRange(
            Expense(skoda, 1450.50m, new DateOnly(2026, 9, 2), 84210),
            Expense(logan, 350m, new DateOnly(2026, 10, 18), 152710, ExpenseKind.Repair),
            Expense(skoda, 120.25m, new DateOnly(2026, 10, 14), 86700, ExpenseKind.Other));
        await db.SaveChangesAsync();

        var total = await SummaryCalculator.TotalAsync(db.CarExpenses);

        Assert.Equal(3, total.Count);
        Assert.Equal(1920.75m, total.Total, 2);
    }

    [Fact]
    public async Task ExpensesOfDifferentMonthsGoToDifferentRows()
    {
        db.CarExpenses.AddRange(
            Expense(skoda, 100m, new DateOnly(2026, 9, 30), 84000),
            Expense(skoda, 200m, new DateOnly(2026, 10, 1), 84100),
            Expense(skoda, 300m, new DateOnly(2026, 10, 20), 84200));
        await db.SaveChangesAsync();

        var result = await SummaryCalculator.ByMonthAsync(db.CarExpenses);

        Assert.Collection(result,
            sep => { Assert.Equal("09.2026", sep.Name); Assert.Equal(1, sep.Count); Assert.Equal(100m, sep.Total, 2); },
            oct => { Assert.Equal("10.2026", oct.Name); Assert.Equal(2, oct.Count); Assert.Equal(500m, oct.Total, 2); });
    }

    [Fact]
    public async Task SingleExpenseGivesExactlyOneRowWithCountOne()
    {
        db.CarExpenses.Add(Expense(logan, 1610m, new DateOnly(2026, 10, 2), 152300));
        await db.SaveChangesAsync();

        var result = await SummaryCalculator.ByVehicleAsync(db.CarExpenses);

        var row = Assert.Single(result);
        Assert.Equal(1, row.Count);
        Assert.Equal(1610m, row.Total, 2);
    }

    [Fact]
    public async Task VehiclesAreOrderedByTotalDescending()
    {
        db.CarExpenses.AddRange(
            Expense(logan, 100m, new DateOnly(2026, 10, 2), 152300),
            Expense(skoda, 900m, new DateOnly(2026, 10, 3), 84000));
        await db.SaveChangesAsync();

        var result = await SummaryCalculator.ByVehicleAsync(db.CarExpenses);

        Assert.Equal(["Skoda Octavia", "Renault Logan"], result.Select(r => r.Name));
    }

    [Fact]
    public async Task FuelConsumptionIsLitersPerHundredKilometres()
    {
        // 1000 км між першим і останнім записом, 60 л пального → 6 л/100 км.
        db.CarExpenses.AddRange(
            Expense(skoda, 1500m, new DateOnly(2026, 9, 1), 80000, liters: 30m),
            Expense(skoda, 1500m, new DateOnly(2026, 9, 15), 81000, liters: 30m),
            Expense(skoda, 2000m, new DateOnly(2026, 9, 20), 80500, ExpenseKind.Maintenance));
        await db.SaveChangesAsync();

        var row = Assert.Single(await SummaryCalculator.FuelByVehicleAsync(db.CarExpenses));

        Assert.Equal(1000, row.Distance);
        Assert.Equal(6m, row.LitersPer100Km!.Value, 2);
        Assert.Equal(3000m, row.FuelCost, 2);
        Assert.Equal(5m, row.CostPerKm!.Value, 2);   // 5000 грн усіх витрат / 1000 км
    }

    [Fact]
    public async Task FuelRowWithoutMileageHasNoConsumption()
    {
        db.CarExpenses.Add(Expense(logan, 1610m, new DateOnly(2026, 10, 2), 152300, liters: 28m));
        await db.SaveChangesAsync();

        var row = Assert.Single(await SummaryCalculator.FuelByVehicleAsync(db.CarExpenses));

        Assert.Equal(0, row.Distance);
        Assert.Null(row.LitersPer100Km);
        Assert.Null(row.CostPerKm);
    }
}
