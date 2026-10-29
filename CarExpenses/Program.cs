using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using CarExpenses.Api;
using CarExpenses.Components;
using CarExpenses.Data;
using CarExpenses.Models;
using CarExpenses.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

builder.Services.AddDbContextFactory<CarExpensesContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddQuickGridEntityFrameworkAdapter();

builder.Services.AddOpenApi();

var app = builder.Build();

// У контейнері й опублікованій збірці засобу dotnet ef немає –
// міграції застосовує сам застосунок під час запуску.
var factory = app.Services.GetRequiredService<IDbContextFactory<CarExpensesContext>>();
await using (var db = await factory.CreateDbContextAsync())
{
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Сторінку «не знайдено» показуємо лише людям; вебсервіс віддає чистий код 404.
app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/api"),
    b => b.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();

if (app.Environment.IsDevelopment())
{
    // HTTPS – лише в розробці: у контейнері сертифіката немає (ЛР 13).
    app.UseHttpsRedirection();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// ---------- вебсервіс (ЛР 11) ----------
var api = app.MapGroup("/api");

api.MapGet("/carexpenses",
    async Task<Ok<List<CarExpenseDto>>> (IDbContextFactory<CarExpensesContext> f) =>
{
    await using var db = await f.CreateDbContextAsync();
    var data = await db.CarExpenses
        .OrderByDescending(x => x.SpentOn)
        .Select(x => new CarExpenseDto(x.Id, x.SpentOn, x.Title, x.Kind.ToString(), x.Amount,
                                       x.Odometer, x.Liters, x.Vehicle!.Name, x.Place!.Name))
        .ToListAsync();
    return TypedResults.Ok(data);
});

api.MapGet("/carexpenses/{id:int}",
    async Task<Results<Ok<CarExpenseDto>, NotFound>> (
        int id, IDbContextFactory<CarExpensesContext> f) =>
{
    await using var db = await f.CreateDbContextAsync();
    var expense = await db.CarExpenses
        .Where(x => x.Id == id)
        .Select(x => new CarExpenseDto(x.Id, x.SpentOn, x.Title, x.Kind.ToString(), x.Amount,
                                       x.Odometer, x.Liters, x.Vehicle!.Name, x.Place!.Name))
        .FirstOrDefaultAsync();
    return expense is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(expense);
});

api.MapGet("/summary",
    async Task<Ok<List<SummaryRow>>> (IDbContextFactory<CarExpensesContext> f) =>
{
    await using var db = await f.CreateDbContextAsync();
    return TypedResults.Ok(await SummaryCalculator.ByVehicleAsync(db.CarExpenses));
});

// ---------- особливість проєкту: експорт у CSV ----------
app.MapGet("/export/carexpenses.csv",
    async (string? search, int? vehicleId, IDbContextFactory<CarExpensesContext> f) =>
{
    await using var db = await f.CreateDbContextAsync();

    var q = db.CarExpenses.Include(x => x.Vehicle).Include(x => x.Place).AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var s = search.Trim().ToLowerInvariant();
        q = q.Where(x => x.TitleSearch.Contains(s));
    }

    if (vehicleId is int id)
        q = q.Where(x => x.VehicleId == id);

    var rows = await q.OrderBy(x => x.SpentOn).ThenBy(x => x.Odometer).ToListAsync();

    // Позначка BOM потрібна, щоб Excel прочитав кирилицю як UTF-8.
    var bytes = Encoding.UTF8.GetPreamble()
        .Concat(Encoding.UTF8.GetBytes(CsvExporter.Build(rows)))
        .ToArray();

    return Results.File(bytes, "text/csv; charset=utf-8",
        $"car-expenses-{DateTime.Today:yyyy-MM-dd}.csv");
}).ExcludeFromDescription();

app.Run();
