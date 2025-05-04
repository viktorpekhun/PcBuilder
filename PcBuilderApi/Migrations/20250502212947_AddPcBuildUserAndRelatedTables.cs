using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilderApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPcBuildUserAndRelatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommentBanUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostBanUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PcBuilds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CpuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GpuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MotherboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CpuCooledId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PowerSupplyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PcCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcBuilds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcBuilds_CpuCoolers_CpuCooledId",
                        column: x => x.CpuCooledId,
                        principalTable: "CpuCoolers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcBuilds_Cpus_CpuId",
                        column: x => x.CpuId,
                        principalTable: "Cpus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcBuilds_Gpus_GpuId",
                        column: x => x.GpuId,
                        principalTable: "Gpus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcBuilds_Motherboards_MotherboardId",
                        column: x => x.MotherboardId,
                        principalTable: "Motherboards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcBuilds_PcCases_PcCaseId",
                        column: x => x.PcCaseId,
                        principalTable: "PcCases",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcBuilds_PowerSupplies_PowerSupplyId",
                        column: x => x.PowerSupplyId,
                        principalTable: "PowerSupplies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcBuilds_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PcBuild_Fan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    FanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PcBuildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcBuild_Fan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcBuild_Fan_Fans_FanId",
                        column: x => x.FanId,
                        principalTable: "Fans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PcBuild_Fan_PcBuilds_PcBuildId",
                        column: x => x.PcBuildId,
                        principalTable: "PcBuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PcBuild_Hdd",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    HddId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PcBuildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcBuild_Hdd", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcBuild_Hdd_Hdds_HddId",
                        column: x => x.HddId,
                        principalTable: "Hdds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PcBuild_Hdd_PcBuilds_PcBuildId",
                        column: x => x.PcBuildId,
                        principalTable: "PcBuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PcBuild_Ram",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    RamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PcBuildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcBuild_Ram", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcBuild_Ram_PcBuilds_PcBuildId",
                        column: x => x.PcBuildId,
                        principalTable: "PcBuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PcBuild_Ram_Rams_RamId",
                        column: x => x.RamId,
                        principalTable: "Rams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PcBuild_Ssd",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SsdId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PcBuildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcBuild_Ssd", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcBuild_Ssd_PcBuilds_PcBuildId",
                        column: x => x.PcBuildId,
                        principalTable: "PcBuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PcBuild_Ssd_Ssds_SsdId",
                        column: x => x.SsdId,
                        principalTable: "Ssds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PcBuildId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_PcBuilds_PcBuildId",
                        column: x => x.PcBuildId,
                        principalTable: "PcBuilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Fan_FanId",
                table: "PcBuild_Fan",
                column: "FanId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Fan_PcBuildId",
                table: "PcBuild_Fan",
                column: "PcBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Hdd_HddId",
                table: "PcBuild_Hdd",
                column: "HddId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Hdd_PcBuildId",
                table: "PcBuild_Hdd",
                column: "PcBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Ram_PcBuildId",
                table: "PcBuild_Ram",
                column: "PcBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Ram_RamId",
                table: "PcBuild_Ram",
                column: "RamId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Ssd_PcBuildId",
                table: "PcBuild_Ssd",
                column: "PcBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Ssd_SsdId",
                table: "PcBuild_Ssd",
                column: "SsdId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_CpuCooledId",
                table: "PcBuilds",
                column: "CpuCooledId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_CpuId",
                table: "PcBuilds",
                column: "CpuId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_GpuId",
                table: "PcBuilds",
                column: "GpuId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_MotherboardId",
                table: "PcBuilds",
                column: "MotherboardId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_PcCaseId",
                table: "PcBuilds",
                column: "PcCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_PowerSupplyId",
                table: "PcBuilds",
                column: "PowerSupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_UserId",
                table: "PcBuilds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_PcBuildId",
                table: "Reviews",
                column: "PcBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PcBuild_Fan");

            migrationBuilder.DropTable(
                name: "PcBuild_Hdd");

            migrationBuilder.DropTable(
                name: "PcBuild_Ram");

            migrationBuilder.DropTable(
                name: "PcBuild_Ssd");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "PcBuilds");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
