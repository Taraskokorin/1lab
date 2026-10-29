using CarExpenses.Models;
using CarExpenses.Services;

namespace CarExpenses.Tests;

public class CsvExporterTests
{
    [Fact]
    public void FirstLineIsUkrainianHeader()
    {
        var csv = CsvExporter.Build([]);

        Assert.StartsWith("Дата;Вид;Що оплачено;Сума, грн", csv);
    }

    [Theory]
    [InlineData("Мийка; салон", "\"Мийка; салон\"")]
    [InlineData("Масло \"Castrol\"", "\"Масло \"\"Castrol\"\"\"")]
    [InlineData("Заправка А-95", "Заправка А-95")]
    public void TitleIsEscapedOnlyWhenNeeded(string title, string expected)
    {
        var expense = new CarExpense
        {
            Title = title, Kind = ExpenseKind.Other, Amount = 250m,
            SpentOn = new DateOnly(2026, 9, 15), Odometer = 84810
        };

        var line = CsvExporter.Build([expense]).Split(Environment.NewLine)[1];

        Assert.Equal($"15.09.2026;Інше;{expected};250,00;84810;;;;", line);
    }
}
