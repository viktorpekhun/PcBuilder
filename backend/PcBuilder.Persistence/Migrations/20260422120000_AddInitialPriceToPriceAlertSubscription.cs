using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilder.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialPriceToPriceAlertSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InitialPrice",
                table: "PriceAlertSubscriptions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                "UPDATE [PriceAlertSubscriptions] SET [InitialPrice] = [LastNotifiedPrice];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialPrice",
                table: "PriceAlertSubscriptions");
        }
    }
}
