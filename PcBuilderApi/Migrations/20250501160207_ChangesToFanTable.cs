using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilderApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangesToFanTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AirFlowCfm",
                table: "Fans",
                newName: "AirflowCfm");

            migrationBuilder.RenameColumn(
                name: "Width",
                table: "Fans",
                newName: "SizeWidth");

            migrationBuilder.RenameColumn(
                name: "NoiseDb",
                table: "Fans",
                newName: "SizeLength");

            migrationBuilder.RenameColumn(
                name: "Length",
                table: "Fans",
                newName: "SizeHeight");

            migrationBuilder.RenameColumn(
                name: "Height",
                table: "Fans",
                newName: "NoiseLevelDb");

            migrationBuilder.AlterColumn<string>(
                name: "SpeedControl",
                table: "Fans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Connector",
                table: "Fans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "Fans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AirflowCfm",
                table: "Fans",
                newName: "AirFlowCfm");

            migrationBuilder.RenameColumn(
                name: "SizeWidth",
                table: "Fans",
                newName: "Width");

            migrationBuilder.RenameColumn(
                name: "SizeLength",
                table: "Fans",
                newName: "NoiseDb");

            migrationBuilder.RenameColumn(
                name: "SizeHeight",
                table: "Fans",
                newName: "Length");

            migrationBuilder.RenameColumn(
                name: "NoiseLevelDb",
                table: "Fans",
                newName: "Height");

            migrationBuilder.AlterColumn<string>(
                name: "SpeedControl",
                table: "Fans",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Connector",
                table: "Fans",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Color",
                table: "Fans",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
