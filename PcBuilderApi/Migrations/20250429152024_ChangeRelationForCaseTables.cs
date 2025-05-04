using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilderApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRelationForCaseTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Case_FanLocation");

            migrationBuilder.DropTable(
                name: "Case_FormFactor");

            migrationBuilder.DropTable(
                name: "FanLocations");

            migrationBuilder.DropTable(
                name: "FormFactors");

            migrationBuilder.CreateTable(
                name: "CaseFanLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FanSize = table.Column<int>(type: "int", nullable: false),
                    MaxFans = table.Column<int>(type: "int", nullable: false),
                    CaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseFanLocations");

            migrationBuilder.DropTable(
                name: "CaseFormFactors");

            migrationBuilder.CreateTable(
                name: "FanLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FanLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormFactors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormFactors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Case_FanLocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FanLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FanSize = table.Column<int>(type: "int", nullable: false),
                    MaxFans = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Case_FanLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Case_FanLocation_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Case_FanLocation_FanLocations_FanLocationId",
                        column: x => x.FanLocationId,
                        principalTable: "FanLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Case_FormFactor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormFactorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Case_FormFactor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Case_FormFactor_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Case_FormFactor_FormFactors_FormFactorId",
                        column: x => x.FormFactorId,
                        principalTable: "FormFactors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Case_FanLocation_CaseId",
                table: "Case_FanLocation",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Case_FanLocation_FanLocationId",
                table: "Case_FanLocation",
                column: "FanLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Case_FormFactor_CaseId",
                table: "Case_FormFactor",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Case_FormFactor_FormFactorId",
                table: "Case_FormFactor",
                column: "FormFactorId");
        }
    }
}
