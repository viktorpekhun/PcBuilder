using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilder.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangePowerSupplyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsModular",
                table: "PowerSupplies");

            migrationBuilder.AddColumn<string>(
                name: "Modularity",
                table: "PowerSupplies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Modularity",
                table: "PowerSupplies");

            migrationBuilder.AddColumn<bool>(
                name: "IsModular",
                table: "PowerSupplies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
