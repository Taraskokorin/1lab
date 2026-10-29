namespace CarExpenses.Api;

/// <summary>Витрата у відповіді вебсервісу: лише те, що потрібно клієнтові.</summary>
public record CarExpenseDto(
    int Id,
    DateOnly SpentOn,
    string Title,
    string Kind,
    decimal Amount,
    int Odometer,
    decimal? Liters,
    string Vehicle,
    string Place);
