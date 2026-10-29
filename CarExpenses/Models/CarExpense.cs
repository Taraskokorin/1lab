namespace CarExpenses.Models;

public class CarExpense
{
    public int Id { get; set; }
    public string Title { get; set; } = "";          // обов’язкова: що саме оплачено
    public ExpenseKind Kind { get; set; }
    public decimal Amount { get; set; }              // сума, грн
    public DateOnly SpentOn { get; set; }
    public int Odometer { get; set; }                // пробіг на момент витрати, км
    public decimal? Liters { get; set; }             // лише для заправок
    public DateOnly? NextServiceOn { get; set; }     // коли наступне ТО, якщо відомо
    public string? Comment { get; set; }
}
