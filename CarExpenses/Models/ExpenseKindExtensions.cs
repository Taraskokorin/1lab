namespace CarExpenses.Models;

public static class ExpenseKindExtensions
{
    /// <summary>Назва виду витрати українською – для показу людині.</summary>
    public static string ToUkrainian(this ExpenseKind kind) => kind switch
    {
        ExpenseKind.Fuel => "Пальне",
        ExpenseKind.Maintenance => "Технічне обслуговування",
        ExpenseKind.Repair => "Ремонт",
        ExpenseKind.Insurance => "Страхування",
        _ => "Інше"
    };
}
