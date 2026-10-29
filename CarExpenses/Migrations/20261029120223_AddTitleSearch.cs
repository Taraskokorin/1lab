using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarExpenses.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TitleSearch",
                table: "CarExpenses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Заповнити службову властивість для наявних записів (lower у SQLite
            // змінює лише латиницю; кириличні записи перезберігаються кодом C#).
            migrationBuilder.Sql("UPDATE CarExpenses SET TitleSearch = lower(Title)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TitleSearch",
                table: "CarExpenses");
        }
    }
}
