using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilderApi.Migrations
{
    /// <inheritdoc />
    public partial class StructureChangesToPcCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxCpuCoolerHeigth",
                table: "PcCases",
                newName: "MaxCpuCoolerHeight");

            migrationBuilder.AlterColumn<double>(
                name: "Weight",
                table: "PcCases",
                type: "float",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxCpuCoolerHeight",
                table: "PcCases",
                newName: "MaxCpuCoolerHeigth");

            migrationBuilder.AlterColumn<int>(
                name: "Weight",
                table: "PcCases",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);
        }
    }
}
