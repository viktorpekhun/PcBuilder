using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilder.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDescriptionsFromComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Ssds");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Rams");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PowerSupplies");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PcCases");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Motherboards");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Hdds");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Gpus");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Fans");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Cpus");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CpuCoolers");

            migrationBuilder.AlterColumn<string>(
                name: "Modularity",
                table: "PowerSupplies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Ssds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Rams",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AlterColumn<string>(
                name: "Modularity",
                table: "PowerSupplies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PowerSupplies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PcCases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Motherboards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Hdds",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Gpus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Fans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Cpus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CpuCoolers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");
        }
    }
}
