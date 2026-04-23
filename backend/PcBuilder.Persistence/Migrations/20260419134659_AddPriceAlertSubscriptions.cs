using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilder.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceAlertSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriceAlertSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentType = table.Column<int>(type: "int", nullable: false),
                    ThresholdPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    LastNotifiedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceAlertSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceAlertSubscriptions_ComponentId_ComponentType",
                table: "PriceAlertSubscriptions",
                columns: new[] { "ComponentId", "ComponentType" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceAlertSubscriptions_UserId_ComponentId_ComponentType",
                table: "PriceAlertSubscriptions",
                columns: new[] { "UserId", "ComponentId", "ComponentType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceAlertSubscriptions");
        }
    }
}
