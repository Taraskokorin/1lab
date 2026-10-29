using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarExpenses.Migrations
{
    /// <inheritdoc />
    public partial class AddReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CarExpenses_SpentOn_Odometer_Kind",
                table: "CarExpenses");

            migrationBuilder.AddColumn<int>(
                name: "PlaceId",
                table: "CarExpenses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "CarExpenses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Places",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Places", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    PlateNumber = table.Column<string>(type: "TEXT", maxLength: 12, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarExpenses_PlaceId",
                table: "CarExpenses",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_CarExpenses_VehicleId_SpentOn_Odometer_Kind",
                table: "CarExpenses",
                columns: new[] { "VehicleId", "SpentOn", "Odometer", "Kind" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CarExpenses_Places_PlaceId",
                table: "CarExpenses",
                column: "PlaceId",
                principalTable: "Places",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CarExpenses_Vehicles_VehicleId",
                table: "CarExpenses",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarExpenses_Places_PlaceId",
                table: "CarExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_CarExpenses_Vehicles_VehicleId",
                table: "CarExpenses");

            migrationBuilder.DropTable(
                name: "Places");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_CarExpenses_PlaceId",
                table: "CarExpenses");

            migrationBuilder.DropIndex(
                name: "IX_CarExpenses_VehicleId_SpentOn_Odometer_Kind",
                table: "CarExpenses");

            migrationBuilder.DropColumn(
                name: "PlaceId",
                table: "CarExpenses");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "CarExpenses");

            migrationBuilder.CreateIndex(
                name: "IX_CarExpenses_SpentOn_Odometer_Kind",
                table: "CarExpenses",
                columns: new[] { "SpentOn", "Odometer", "Kind" },
                unique: true);
        }
    }
}
