using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarExpenses.Migrations
{
    /// <inheritdoc />
    public partial class UniqueExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CarExpenses_SpentOn_Odometer_Kind",
                table: "CarExpenses",
                columns: new[] { "SpentOn", "Odometer", "Kind" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CarExpenses_SpentOn_Odometer_Kind",
                table: "CarExpenses");
        }
    }
}
