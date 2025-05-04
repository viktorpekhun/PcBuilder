using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilderApi.Migrations
{
    /// <inheritdoc />
    public partial class AddNewRowToMotherboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PcleX1Quantity",
                table: "Motherboards",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PcleX1Quantity",
                table: "Motherboards");
        }
    }
}
