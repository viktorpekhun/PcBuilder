using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilder.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixMotherboardTablesNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CpuPowerConnectors_Motherboards_MotherboardId",
                table: "CpuPowerConnectors");

            migrationBuilder.DropForeignKey(
                name: "FK_InnerPorts_Motherboards_MotherboardId",
                table: "InnerPorts");

            migrationBuilder.DropForeignKey(
                name: "FK_M2Slots_Motherboards_MotherboardId",
                table: "M2Slots");

            migrationBuilder.DropForeignKey(
                name: "FK_PcleSlots_Motherboards_MotherboardId",
                table: "PcleSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_RearPorts_Motherboards_MotherboardId",
                table: "RearPorts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RearPorts",
                table: "RearPorts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PcleSlots",
                table: "PcleSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_M2Slots",
                table: "M2Slots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InnerPorts",
                table: "InnerPorts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CpuPowerConnectors",
                table: "CpuPowerConnectors");

            migrationBuilder.RenameTable(
                name: "RearPorts",
                newName: "MotherboardRearPorts");

            migrationBuilder.RenameTable(
                name: "PcleSlots",
                newName: "MotherboardPcleSlots");

            migrationBuilder.RenameTable(
                name: "M2Slots",
                newName: "MotherboardM2Slots");

            migrationBuilder.RenameTable(
                name: "InnerPorts",
                newName: "MotherboardInnerPorts");

            migrationBuilder.RenameTable(
                name: "CpuPowerConnectors",
                newName: "MotherboardCpuPowerConnectors");

            migrationBuilder.RenameIndex(
                name: "IX_RearPorts_MotherboardId",
                table: "MotherboardRearPorts",
                newName: "IX_MotherboardRearPorts_MotherboardId");

            migrationBuilder.RenameIndex(
                name: "IX_PcleSlots_MotherboardId",
                table: "MotherboardPcleSlots",
                newName: "IX_MotherboardPcleSlots_MotherboardId");

            migrationBuilder.RenameIndex(
                name: "IX_M2Slots_MotherboardId",
                table: "MotherboardM2Slots",
                newName: "IX_MotherboardM2Slots_MotherboardId");

            migrationBuilder.RenameIndex(
                name: "IX_InnerPorts_MotherboardId",
                table: "MotherboardInnerPorts",
                newName: "IX_MotherboardInnerPorts_MotherboardId");

            migrationBuilder.RenameIndex(
                name: "IX_CpuPowerConnectors_MotherboardId",
                table: "MotherboardCpuPowerConnectors",
                newName: "IX_MotherboardCpuPowerConnectors_MotherboardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MotherboardRearPorts",
                table: "MotherboardRearPorts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MotherboardPcleSlots",
                table: "MotherboardPcleSlots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MotherboardM2Slots",
                table: "MotherboardM2Slots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MotherboardInnerPorts",
                table: "MotherboardInnerPorts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MotherboardCpuPowerConnectors",
                table: "MotherboardCpuPowerConnectors",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MotherboardCpuPowerConnectors_Motherboards_MotherboardId",
                table: "MotherboardCpuPowerConnectors",
                column: "MotherboardId",
                principalTable: "Motherboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MotherboardInnerPorts_Motherboards_MotherboardId",
                table: "MotherboardInnerPorts",
                column: "MotherboardId",
                principalTable: "Motherboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MotherboardM2Slots_Motherboards_MotherboardId",
                table: "MotherboardM2Slots",
                column: "MotherboardId",
                principalTable: "Motherboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MotherboardPcleSlots_Motherboards_MotherboardId",
                table: "MotherboardPcleSlots",
                column: "MotherboardId",
                principalTable: "Motherboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MotherboardRearPorts_Motherboards_MotherboardId",
                table: "MotherboardRearPorts",
                column: "MotherboardId",
                principalTable: "Motherboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MotherboardCpuPowerConnectors_Motherboards_MotherboardId",
                table: "MotherboardCpuPowerConnectors");

            migrationBuilder.DropForeignKey(
                name: "FK_MotherboardInnerPorts_Motherboards_MotherboardId",
                table: "MotherboardInnerPorts");

            migrationBuilder.DropForeignKey(
                name: "FK_MotherboardM2Slots_Motherboards_MotherboardId",
                table: "MotherboardM2Slots");

            migrationBuilder.DropForeignKey(
                name: "FK_MotherboardPcleSlots_Motherboards_MotherboardId",
                table: "MotherboardPcleSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_MotherboardRearPorts_Motherboards_MotherboardId",
                table: "MotherboardRearPorts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MotherboardRearPorts",
                table: "MotherboardRearPorts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MotherboardPcleSlots",
                table: "MotherboardPcleSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MotherboardM2Slots",
                table: "MotherboardM2Slots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MotherboardInnerPorts",
                table: "MotherboardInnerPorts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MotherboardCpuPowerConnectors",
                table: "MotherboardCpuPowerConnectors");

            migrationBuilder.RenameTable(
                name: "MotherboardRearPorts",
                newName: "RearPorts");

            migrationBuilder.RenameTable(
                name: "MotherboardPcleSlots",
                newName: "PcleSlots");

            migrationBuilder.RenameTable(
                name: "MotherboardM2Slots",
                newName: "M2Slots");

            migrationBuilder.RenameTable(
                name: "MotherboardInnerPorts",
                newName: "InnerPorts");

            migrationBuilder.RenameTable(
                name: "MotherboardCpuPowerConnectors",
                newName: "CpuPowerConnectors");

            migrationBuilder.RenameIndex(
                name: "IX_MotherboardRearPorts_MotherboardId",
                table: "RearPorts",
                newName: "IX_RearPorts_MotherboardId");

            migrationBuilder.RenameIndex(
                name: "IX_MotherboardPcleSlots_MotherboardId",
                table: "PcleSlots",
                newName: "IX_PcleSlots_MotherboardId");

            migrationBuilder.RenameIndex(
                name: "IX_MotherboardM2Slots_MotherboardId",
                table: "M2Slots",
                newName: "IX_M2Slots_MotherboardId");

            migrationBuilder.RenameIndex(
                name: "IX_MotherboardInnerPorts_MotherboardId",
                table: "InnerPorts",
                newName: "IX_InnerPorts_MotherboardId");

            migrationBuilder.RenameIndex(
                name: "IX_MotherboardCpuPowerConnectors_MotherboardId",
                table: "CpuPowerConnectors",
                newName: "IX_CpuPowerConnectors_MotherboardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RearPorts",
                table: "RearPorts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PcleSlots",
                table: "PcleSlots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_M2Slots",
                table: "M2Slots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InnerPorts",
                table: "InnerPorts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CpuPowerConnectors",
                table: "CpuPowerConnectors",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CpuPowerConnectors_Motherboards_MotherboardId",
                table: "CpuPowerConnectors",
                column: "MotherboardId",
                principalTable: "Motherboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InnerPorts_Motherboards_MotherboardId",
                table: "InnerPorts",
                column: "MotherboardId",
                principalTable: "Motherboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_M2Slots_Motherboards_MotherboardId",
                table: "M2Slots",
                column: "MotherboardId",
                principalTable: "Motherboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PcleSlots_Motherboards_MotherboardId",
                table: "PcleSlots",
                column: "MotherboardId",
                principalTable: "Motherboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RearPorts_Motherboards_MotherboardId",
                table: "RearPorts",
                column: "MotherboardId",
                principalTable: "Motherboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
