using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilderApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCountToComponnents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OffersCount",
                table: "Ssds",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OffersCount",
                table: "Rams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OffersCount",
                table: "PowerSupplies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OffersCount",
                table: "PcCases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OffersCount",
                table: "Motherboards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OffersCount",
                table: "Hdds",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OffersCount",
                table: "Gpus",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OffersCount",
                table: "Fans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OffersCount",
                table: "Cpus",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OffersCount",
                table: "CpuCoolers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OffersCount",
                table: "Ssds");

            migrationBuilder.DropColumn(
                name: "OffersCount",
                table: "Rams");

            migrationBuilder.DropColumn(
                name: "OffersCount",
                table: "PowerSupplies");

            migrationBuilder.DropColumn(
                name: "OffersCount",
                table: "PcCases");

            migrationBuilder.DropColumn(
                name: "OffersCount",
                table: "Motherboards");

            migrationBuilder.DropColumn(
                name: "OffersCount",
                table: "Hdds");

            migrationBuilder.DropColumn(
                name: "OffersCount",
                table: "Gpus");

            migrationBuilder.DropColumn(
                name: "OffersCount",
                table: "Fans");

            migrationBuilder.DropColumn(
                name: "OffersCount",
                table: "Cpus");

            migrationBuilder.DropColumn(
                name: "OffersCount",
                table: "CpuCoolers");
        }
    }
}
