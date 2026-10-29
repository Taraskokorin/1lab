using System.ComponentModel.DataAnnotations;

namespace CarExpenses.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Зазначте назву автомобіля")]
    [StringLength(80, ErrorMessage = "Назва задовга")]
    public string Name { get; set; } = "";

    [StringLength(12, ErrorMessage = "Номерний знак задовгий")]
    public string? PlateNumber { get; set; }
}
