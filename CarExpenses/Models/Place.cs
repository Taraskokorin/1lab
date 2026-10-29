using System.ComponentModel.DataAnnotations;

namespace CarExpenses.Models;

/// <summary>Де зроблено витрату: АЗС, СТО, мийка, страхова компанія.</summary>
public class Place
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Зазначте назву місця")]
    [StringLength(80, ErrorMessage = "Назва задовга")]
    public string Name { get; set; } = "";
}
