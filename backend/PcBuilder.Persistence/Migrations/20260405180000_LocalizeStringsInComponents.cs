using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilder.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LocalizeStringsInComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Rams");

            migrationBuilder.DropColumn(
                name: "Modularity",
                table: "PowerSupplies");

            migrationBuilder.DropColumn(
                name: "AdditionalFanPlaces",
                table: "PcCases");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Fans");

            migrationBuilder.RenameColumn(
                name: "BearingType",
                table: "Fans",
                newName: "Color_Uk");

            migrationBuilder.RenameColumn(
                name: "RadiatorMaterial",
                table: "CpuCoolers",
                newName: "RadiatorMaterial_Uk");

            migrationBuilder.AddColumn<string>(
                name: "Color_En",
                table: "Rams",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color_Uk",
                table: "Rams",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modularity_En",
                table: "PowerSupplies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modularity_Uk",
                table: "PowerSupplies",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalFanPlaces_En",
                table: "PcCases",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalFanPlaces_Uk",
                table: "PcCases",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BearingType_En",
                table: "Fans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BearingType_Uk",
                table: "Fans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color_En",
                table: "Fans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RadiatorMaterial_En",
                table: "CpuCoolers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color_En",
                table: "Rams");

            migrationBuilder.DropColumn(
                name: "Color_Uk",
                table: "Rams");

            migrationBuilder.DropColumn(
                name: "Modularity_En",
                table: "PowerSupplies");

            migrationBuilder.DropColumn(
                name: "Modularity_Uk",
                table: "PowerSupplies");

            migrationBuilder.DropColumn(
                name: "AdditionalFanPlaces_En",
                table: "PcCases");

            migrationBuilder.DropColumn(
                name: "AdditionalFanPlaces_Uk",
                table: "PcCases");

            migrationBuilder.DropColumn(
                name: "BearingType_En",
                table: "Fans");

            migrationBuilder.DropColumn(
                name: "BearingType_Uk",
                table: "Fans");

            migrationBuilder.DropColumn(
                name: "Color_En",
                table: "Fans");

            migrationBuilder.DropColumn(
                name: "RadiatorMaterial_En",
                table: "CpuCoolers");

            migrationBuilder.RenameColumn(
                name: "Color_Uk",
                table: "Fans",
                newName: "BearingType");

            migrationBuilder.RenameColumn(
                name: "RadiatorMaterial_Uk",
                table: "CpuCoolers",
                newName: "RadiatorMaterial");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Rams",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modularity",
                table: "PowerSupplies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalFanPlaces",
                table: "PcCases",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Fans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
