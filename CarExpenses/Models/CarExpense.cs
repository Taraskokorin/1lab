using System.ComponentModel.DataAnnotations;

namespace CarExpenses.Models;

public class CarExpense
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Зазначте, що саме оплачено")]
    [StringLength(150, ErrorMessage = "Опис задовгий: не більше 150 символів")]
    public string Title { get; set; } = "";          // обов’язкова: що саме оплачено

    public ExpenseKind Kind { get; set; }

    [Range(0.01, 1_000_000, ErrorMessage = "Сума має бути від 0,01 до 1 000 000 грн")]
    public decimal Amount { get; set; }              // сума, грн

    public DateOnly SpentOn { get; set; }

    [Range(0, 2_000_000, ErrorMessage = "Пробіг має бути від 0 до 2 000 000 км")]
    public int Odometer { get; set; }                // пробіг на момент витрати, км

    [Range(0.1, 200, ErrorMessage = "Об’єм заправки має бути від 0,1 до 200 л")]
    public decimal? Liters { get; set; }             // лише для заправок

    public DateOnly? NextServiceOn { get; set; }     // коли наступне ТО, якщо відомо

    [StringLength(500, ErrorMessage = "Коментар задовгий: не більше 500 символів")]
    public string? Comment { get; set; }

    public int VehicleId { get; set; }               // зовнішній ключ
    public Vehicle? Vehicle { get; set; }            // навігаційна властивість
    public int PlaceId { get; set; }
    public Place? Place { get; set; }
}
