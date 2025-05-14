using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilderApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAveragePriceToComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AveragePrice",
                table: "Ssds",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePrice",
                table: "Rams",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePrice",
                table: "PowerSupplies",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePrice",
                table: "PcCases",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePrice",
                table: "Motherboards",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePrice",
                table: "Hdds",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePrice",
                table: "Gpus",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePrice",
                table: "Fans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePrice",
                table: "Cpus",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePrice",
                table: "CpuCoolers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AveragePrice",
                table: "Ssds");

            migrationBuilder.DropColumn(
                name: "AveragePrice",
                table: "Rams");

            migrationBuilder.DropColumn(
                name: "AveragePrice",
                table: "PowerSupplies");

            migrationBuilder.DropColumn(
                name: "AveragePrice",
                table: "PcCases");

            migrationBuilder.DropColumn(
                name: "AveragePrice",
                table: "Motherboards");

            migrationBuilder.DropColumn(
                name: "AveragePrice",
                table: "Hdds");

            migrationBuilder.DropColumn(
                name: "AveragePrice",
                table: "Gpus");

            migrationBuilder.DropColumn(
                name: "AveragePrice",
                table: "Fans");

            migrationBuilder.DropColumn(
                name: "AveragePrice",
                table: "Cpus");

            migrationBuilder.DropColumn(
                name: "AveragePrice",
                table: "CpuCoolers");
        }
    }
}
