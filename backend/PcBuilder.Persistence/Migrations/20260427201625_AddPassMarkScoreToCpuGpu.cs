using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilder.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPassMarkScoreToCpuGpu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PassMarkScore",
                table: "Gpus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PassMarkScore",
                table: "Cpus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Gpus_PassMarkScore",
                table: "Gpus",
                column: "PassMarkScore");

            migrationBuilder.CreateIndex(
                name: "IX_Cpus_PassMarkScore",
                table: "Cpus",
                column: "PassMarkScore");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Gpus_PassMarkScore",
                table: "Gpus");

            migrationBuilder.DropIndex(
                name: "IX_Cpus_PassMarkScore",
                table: "Cpus");

            migrationBuilder.DropColumn(
                name: "PassMarkScore",
                table: "Gpus");

            migrationBuilder.DropColumn(
                name: "PassMarkScore",
                table: "Cpus");
        }
    }
}
