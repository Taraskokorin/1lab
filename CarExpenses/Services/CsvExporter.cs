using System.Globalization;
using System.Text;
using CarExpenses.Models;

namespace CarExpenses.Services;

/// <summary>Особливість проєкту: звіт про витрати у файлі з роздільниками.</summary>
public static class CsvExporter
{
    // Крапка з комою – роздільник, який український Excel розпізнає сам.
    private const char Separator = ';';

    public static string Build(IEnumerable<CarExpense> expenses)
    {
        var csv = new StringBuilder();
        csv.AppendLine(string.Join(Separator,
            "Дата", "Вид", "Що оплачено", "Сума, грн", "Пробіг, км", "Літри", "Автомобіль", "Місце", "Коментар"));

        foreach (var e in expenses)
            csv.AppendLine(string.Join(Separator,
                e.SpentOn.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                Escape(e.Kind.ToUkrainian()),
                Escape(e.Title),
                e.Amount.ToString("0.00", Ukrainian),
                e.Odometer.ToString(CultureInfo.InvariantCulture),
                e.Liters?.ToString("0.##", Ukrainian) ?? "",
                Escape(e.Vehicle?.Name ?? ""),
                Escape(e.Place?.Name ?? ""),
                Escape(e.Comment ?? "")));

        return csv.ToString();
    }

    // Числа – з комою, як їх чекає Excel з українськими налаштуваннями.
    private static readonly CultureInfo Ukrainian = CultureInfo.GetCultureInfo("uk-UA");

    // Значення з роздільником, лапками чи переносом рядка беруть у лапки,
    // а лапки всередині подвоюють – так вимагає формат CSV.
    private static string Escape(string value) =>
        value.IndexOfAny([Separator, '"', '\n', '\r']) >= 0
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
}
