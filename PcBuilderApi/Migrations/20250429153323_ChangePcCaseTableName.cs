using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilderApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangePcCaseTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseFanLocations");

            migrationBuilder.DropTable(
                name: "CaseFormFactors");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.CreateTable(
                name: "PcCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeStandard = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SizeDimentions = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Weight = table.Column<int>(type: "int", nullable: true),
                    PsuWattage = table.Column<int>(type: "int", nullable: true),
                    PsuLocation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MaxGpuLength = table.Column<double>(type: "float", nullable: true),
                    MaxCpuCoolerHeigth = table.Column<double>(type: "float", nullable: true),
                    HasDustFilters = table.Column<bool>(type: "bit", nullable: false),
                    BuiltInFans = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Slot25Quant = table.Column<int>(type: "int", nullable: true),
                    Slot35Quant = table.Column<int>(type: "int", nullable: true),
                    Slot525Quant = table.Column<int>(type: "int", nullable: true),
                    ExpansionSlotQuant = table.Column<int>(type: "int", nullable: true),
                    Usb = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HasHeadphones = table.Column<bool>(type: "bit", nullable: false),
                    HasMicrophone = table.Column<bool>(type: "bit", nullable: false),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcCases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PcCaseFanLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FanSize = table.Column<int>(type: "int", nullable: false),
                    MaxFans = table.Column<int>(type: "int", nullable: false),
                    PcCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcCaseFanLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcCaseFanLocations_PcCases_PcCaseId",
                        column: x => x.PcCaseId,
                        principalTable: "PcCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PcCaseFormFactors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PcCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcCaseFormFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcCaseFormFactors_PcCases_PcCaseId",
                        column: x => x.PcCaseId,
                        principalTable: "PcCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PcCaseFanLocations_PcCaseId",
                table: "PcCaseFanLocations",
                column: "PcCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PcCaseFormFactors_PcCaseId",
                table: "PcCaseFormFactors",
                column: "PcCaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PcCaseFanLocations");

            migrationBuilder.DropTable(
                name: "PcCaseFormFactors");

            migrationBuilder.DropTable(
                name: "PcCases");

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BuiltInFans = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    ExpansionSlotQuant = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasDustFilters = table.Column<bool>(type: "bit", nullable: false),
                    HasHeadphones = table.Column<bool>(type: "bit", nullable: false),
                    HasMicrophone = table.Column<bool>(type: "bit", nullable: false),
                    MaxCpuCoolerHeigth = table.Column<double>(type: "float", nullable: true),
                    MaxGpuLength = table.Column<double>(type: "float", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PsuLocation = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PsuWattage = table.Column<int>(type: "int", nullable: true),
                    SizeDimentions = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SizeStandard = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Slot25Quant = table.Column<int>(type: "int", nullable: true),
                    Slot35Quant = table.Column<int>(type: "int", nullable: true),
                    Slot525Quant = table.Column<int>(type: "int", nullable: true),
                    Usb = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Weight = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaseFanLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FanSize = table.Column<int>(type: "int", nullable: false),
                    MaxFans = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseFanLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseFanLocations_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseFormFactors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseFormFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseFormFactors_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseFanLocations_CaseId",
                table: "CaseFanLocations",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFormFactors_CaseId",
                table: "CaseFormFactors",
                column: "CaseId");
        }
    }
}
