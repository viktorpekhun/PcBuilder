using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilderApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSavingToPcBuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PcBuilds_CpuCoolers_CpuCooledId",
                table: "PcBuilds");

            migrationBuilder.RenameColumn(
                name: "CpuCooledId",
                table: "PcBuilds",
                newName: "PowerSupplyOfferId");

            migrationBuilder.RenameIndex(
                name: "IX_PcBuilds_CpuCooledId",
                table: "PcBuilds",
                newName: "IX_PcBuilds_PowerSupplyOfferId");

            migrationBuilder.AddColumn<Guid>(
                name: "CpuCoolerId",
                table: "PcBuilds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CpuCoolerOfferId",
                table: "PcBuilds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CpuOfferId",
                table: "PcBuilds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GpuOfferId",
                table: "PcBuilds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MotherboardOfferId",
                table: "PcBuilds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PcCaseOfferId",
                table: "PcBuilds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductOfferId",
                table: "PcBuild_Ssd",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductOfferId",
                table: "PcBuild_Ram",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductOfferId",
                table: "PcBuild_Hdd",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductOfferId",
                table: "PcBuild_Fan",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_CpuCoolerId",
                table: "PcBuilds",
                column: "CpuCoolerId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_CpuCoolerOfferId",
                table: "PcBuilds",
                column: "CpuCoolerOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_CpuOfferId",
                table: "PcBuilds",
                column: "CpuOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_GpuOfferId",
                table: "PcBuilds",
                column: "GpuOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_MotherboardOfferId",
                table: "PcBuilds",
                column: "MotherboardOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_PcCaseOfferId",
                table: "PcBuilds",
                column: "PcCaseOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Ssd_ProductOfferId",
                table: "PcBuild_Ssd",
                column: "ProductOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Ram_ProductOfferId",
                table: "PcBuild_Ram",
                column: "ProductOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Hdd_ProductOfferId",
                table: "PcBuild_Hdd",
                column: "ProductOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Fan_ProductOfferId",
                table: "PcBuild_Fan",
                column: "ProductOfferId");

            migrationBuilder.AddForeignKey(
                name: "FK_PcBuild_Fan_ProductOffers_ProductOfferId",
                table: "PcBuild_Fan",
                column: "ProductOfferId",
                principalTable: "ProductOffers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PcBuild_Hdd_ProductOffers_ProductOfferId",
                table: "PcBuild_Hdd",
                column: "ProductOfferId",
                principalTable: "ProductOffers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PcBuild_Ram_ProductOffers_ProductOfferId",
                table: "PcBuild_Ram",
                column: "ProductOfferId",
                principalTable: "ProductOffers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PcBuild_Ssd_ProductOffers_ProductOfferId",
                table: "PcBuild_Ssd",
                column: "ProductOfferId",
                principalTable: "ProductOffers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PcBuilds_CpuCoolers_CpuCoolerId",
                table: "PcBuilds",
                column: "CpuCoolerId",
                principalTable: "CpuCoolers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PcBuilds_ProductOffers_CpuCoolerOfferId",
                table: "PcBuilds",
                column: "CpuCoolerOfferId",
                principalTable: "ProductOffers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PcBuilds_ProductOffers_CpuOfferId",
                table: "PcBuilds",
                column: "CpuOfferId",
                principalTable: "ProductOffers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PcBuilds_ProductOffers_GpuOfferId",
                table: "PcBuilds",
                column: "GpuOfferId",
                principalTable: "ProductOffers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PcBuilds_ProductOffers_MotherboardOfferId",
                table: "PcBuilds",
                column: "MotherboardOfferId",
                principalTable: "ProductOffers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PcBuilds_ProductOffers_PcCaseOfferId",
                table: "PcBuilds",
                column: "PcCaseOfferId",
                principalTable: "ProductOffers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PcBuilds_ProductOffers_PowerSupplyOfferId",
                table: "PcBuilds",
                column: "PowerSupplyOfferId",
                principalTable: "ProductOffers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PcBuild_Fan_ProductOffers_ProductOfferId",
                table: "PcBuild_Fan");

            migrationBuilder.DropForeignKey(
                name: "FK_PcBuild_Hdd_ProductOffers_ProductOfferId",
                table: "PcBuild_Hdd");

            migrationBuilder.DropForeignKey(
                name: "FK_PcBuild_Ram_ProductOffers_ProductOfferId",
                table: "PcBuild_Ram");

            migrationBuilder.DropForeignKey(
                name: "FK_PcBuild_Ssd_ProductOffers_ProductOfferId",
                table: "PcBuild_Ssd");

            migrationBuilder.DropForeignKey(
                name: "FK_PcBuilds_CpuCoolers_CpuCoolerId",
                table: "PcBuilds");

            migrationBuilder.DropForeignKey(
                name: "FK_PcBuilds_ProductOffers_CpuCoolerOfferId",
                table: "PcBuilds");

            migrationBuilder.DropForeignKey(
                name: "FK_PcBuilds_ProductOffers_CpuOfferId",
                table: "PcBuilds");

            migrationBuilder.DropForeignKey(
                name: "FK_PcBuilds_ProductOffers_GpuOfferId",
                table: "PcBuilds");

            migrationBuilder.DropForeignKey(
                name: "FK_PcBuilds_ProductOffers_MotherboardOfferId",
                table: "PcBuilds");

            migrationBuilder.DropForeignKey(
                name: "FK_PcBuilds_ProductOffers_PcCaseOfferId",
                table: "PcBuilds");

            migrationBuilder.DropForeignKey(
                name: "FK_PcBuilds_ProductOffers_PowerSupplyOfferId",
                table: "PcBuilds");

            migrationBuilder.DropIndex(
                name: "IX_PcBuilds_CpuCoolerId",
                table: "PcBuilds");

            migrationBuilder.DropIndex(
                name: "IX_PcBuilds_CpuCoolerOfferId",
                table: "PcBuilds");

            migrationBuilder.DropIndex(
                name: "IX_PcBuilds_CpuOfferId",
                table: "PcBuilds");

            migrationBuilder.DropIndex(
                name: "IX_PcBuilds_GpuOfferId",
                table: "PcBuilds");

            migrationBuilder.DropIndex(
                name: "IX_PcBuilds_MotherboardOfferId",
                table: "PcBuilds");

            migrationBuilder.DropIndex(
                name: "IX_PcBuilds_PcCaseOfferId",
                table: "PcBuilds");

            migrationBuilder.DropIndex(
                name: "IX_PcBuild_Ssd_ProductOfferId",
                table: "PcBuild_Ssd");

            migrationBuilder.DropIndex(
                name: "IX_PcBuild_Ram_ProductOfferId",
                table: "PcBuild_Ram");

            migrationBuilder.DropIndex(
                name: "IX_PcBuild_Hdd_ProductOfferId",
                table: "PcBuild_Hdd");

            migrationBuilder.DropIndex(
                name: "IX_PcBuild_Fan_ProductOfferId",
                table: "PcBuild_Fan");

            migrationBuilder.DropColumn(
                name: "CpuCoolerId",
                table: "PcBuilds");

            migrationBuilder.DropColumn(
                name: "CpuCoolerOfferId",
                table: "PcBuilds");

            migrationBuilder.DropColumn(
                name: "CpuOfferId",
                table: "PcBuilds");

            migrationBuilder.DropColumn(
                name: "GpuOfferId",
                table: "PcBuilds");

            migrationBuilder.DropColumn(
                name: "MotherboardOfferId",
                table: "PcBuilds");

            migrationBuilder.DropColumn(
                name: "PcCaseOfferId",
                table: "PcBuilds");

            migrationBuilder.DropColumn(
                name: "ProductOfferId",
                table: "PcBuild_Ssd");

            migrationBuilder.DropColumn(
                name: "ProductOfferId",
                table: "PcBuild_Ram");

            migrationBuilder.DropColumn(
                name: "ProductOfferId",
                table: "PcBuild_Hdd");

            migrationBuilder.DropColumn(
                name: "ProductOfferId",
                table: "PcBuild_Fan");

            migrationBuilder.RenameColumn(
                name: "PowerSupplyOfferId",
                table: "PcBuilds",
                newName: "CpuCooledId");

            migrationBuilder.RenameIndex(
                name: "IX_PcBuilds_PowerSupplyOfferId",
                table: "PcBuilds",
                newName: "IX_PcBuilds_CpuCooledId");

            migrationBuilder.AddForeignKey(
                name: "FK_PcBuilds_CpuCoolers_CpuCooledId",
                table: "PcBuilds",
                column: "CpuCooledId",
                principalTable: "CpuCoolers",
                principalColumn: "Id");
        }
    }
}
