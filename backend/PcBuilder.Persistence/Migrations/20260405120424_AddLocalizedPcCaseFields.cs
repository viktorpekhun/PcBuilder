using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilder.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizedPcCaseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PsuLocation",
                table: "PcCases",
                newName: "PsuLocation_Uk");

            migrationBuilder.RenameColumn(
                name: "BuiltInFans",
                table: "PcCases",
                newName: "BuiltInFans_Uk");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "PcCaseFanLocations",
                newName: "Name_Uk");

            migrationBuilder.AddColumn<string>(
                name: "BuiltInFans_En",
                table: "PcCases",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PsuLocation_En",
                table: "PcCases",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name_En",
                table: "PcCaseFanLocations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuiltInFans_En",
                table: "PcCases");

            migrationBuilder.DropColumn(
                name: "PsuLocation_En",
                table: "PcCases");

            migrationBuilder.DropColumn(
                name: "Name_En",
                table: "PcCaseFanLocations");

            migrationBuilder.RenameColumn(
                name: "PsuLocation_Uk",
                table: "PcCases",
                newName: "PsuLocation");

            migrationBuilder.RenameColumn(
                name: "BuiltInFans_Uk",
                table: "PcCases",
                newName: "BuiltInFans");

            migrationBuilder.RenameColumn(
                name: "Name_Uk",
                table: "PcCaseFanLocations",
                newName: "Name");
        }
    }
}
