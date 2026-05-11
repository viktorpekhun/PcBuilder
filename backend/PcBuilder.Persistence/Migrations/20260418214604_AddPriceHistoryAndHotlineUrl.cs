using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilder.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceHistoryAndHotlineUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HotlineUrl",
                table: "Ssds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotlineUrl",
                table: "Rams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotlineUrl",
                table: "PowerSupplies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotlineUrl",
                table: "PcCases",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotlineUrl",
                table: "Motherboards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotlineUrl",
                table: "Hdds",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotlineUrl",
                table: "Gpus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotlineUrl",
                table: "Fans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotlineUrl",
                table: "Cpus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotlineUrl",
                table: "CpuCoolers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PriceHistoryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentType = table.Column<int>(type: "int", nullable: false),
                    AveragePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OffersCount = table.Column<int>(type: "int", nullable: false),
                    SnapshotDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistoryEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistoryEntries_ComponentId_ComponentType_SnapshotDate",
                table: "PriceHistoryEntries",
                columns: new[] { "ComponentId", "ComponentType", "SnapshotDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceHistoryEntries");

            migrationBuilder.DropColumn(
                name: "HotlineUrl",
                table: "Ssds");

            migrationBuilder.DropColumn(
                name: "HotlineUrl",
                table: "Rams");

            migrationBuilder.DropColumn(
                name: "HotlineUrl",
                table: "PowerSupplies");

            migrationBuilder.DropColumn(
                name: "HotlineUrl",
                table: "PcCases");

            migrationBuilder.DropColumn(
                name: "HotlineUrl",
                table: "Motherboards");

            migrationBuilder.DropColumn(
                name: "HotlineUrl",
                table: "Hdds");

            migrationBuilder.DropColumn(
                name: "HotlineUrl",
                table: "Gpus");

            migrationBuilder.DropColumn(
                name: "HotlineUrl",
                table: "Fans");

            migrationBuilder.DropColumn(
                name: "HotlineUrl",
                table: "Cpus");

            migrationBuilder.DropColumn(
                name: "HotlineUrl",
                table: "CpuCoolers");
        }
    }
}
