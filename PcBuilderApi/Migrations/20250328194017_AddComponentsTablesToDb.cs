using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilderApi.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentsTablesToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
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
                    table.PrimaryKey("PK_Cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CpuCoolers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FanCount = table.Column<int>(type: "int", nullable: true),
                    FanSize = table.Column<double>(type: "float", nullable: true),
                    RadiatorMaterial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SpeedControl = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PowerConnector = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaxPowerDissipation = table.Column<int>(type: "int", nullable: true),
                    MaxSpeed = table.Column<int>(type: "int", nullable: true),
                    MinSpeed = table.Column<int>(type: "int", nullable: true),
                    AirflowCfm = table.Column<double>(type: "float", nullable: true),
                    NoiseLevelDb = table.Column<double>(type: "float", nullable: true),
                    Voltage = table.Column<int>(type: "int", nullable: true),
                    Lifespan = table.Column<int>(type: "int", nullable: true),
                    Length = table.Column<int>(type: "int", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<int>(type: "int", nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CpuCoolers", x => x.Id);
                    table.CheckConstraint("CHK_CpuCooler_Type", "Type IN ('Air', 'Water')");
                });

            migrationBuilder.CreateTable(
                name: "Cpus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Socket = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BasicFrequency = table.Column<double>(type: "float", nullable: true),
                    MaxFrequency = table.Column<double>(type: "float", nullable: true),
                    Cache = table.Column<int>(type: "int", nullable: true),
                    DimmType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Cores = table.Column<int>(type: "int", nullable: true),
                    Threads = table.Column<int>(type: "int", nullable: true),
                    Techprocess = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tdp = table.Column<int>(type: "int", nullable: true),
                    IntegratedGraphics = table.Column<bool>(type: "bit", nullable: false),
                    Complectation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cpus", x => x.Id);
                });

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
                name: "Fans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModuleCount = table.Column<int>(type: "int", nullable: true),
                    BearingType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SpeedControl = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Connector = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MinSpeed = table.Column<int>(type: "int", nullable: true),
                    MaxSpeed = table.Column<int>(type: "int", nullable: true),
                    AirFlowCfm = table.Column<double>(type: "float", nullable: true),
                    NoiseDb = table.Column<double>(type: "float", nullable: true),
                    Voltage = table.Column<int>(type: "int", nullable: true),
                    Length = table.Column<double>(type: "float", nullable: true),
                    Width = table.Column<double>(type: "float", nullable: true),
                    Height = table.Column<double>(type: "float", nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fans", x => x.Id);
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
                name: "Gpus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GpuManufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Memory = table.Column<int>(type: "int", nullable: true),
                    MemoryType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PcleVersion = table.Column<double>(type: "float", nullable: true),
                    PcleLane = table.Column<int>(type: "int", nullable: true),
                    MaxFrequency = table.Column<int>(type: "int", nullable: true),
                    CudaCores = table.Column<int>(type: "int", nullable: true),
                    Threads = table.Column<int>(type: "int", nullable: true),
                    MemorySpeed = table.Column<int>(type: "int", nullable: true),
                    MemoryBus = table.Column<int>(type: "int", nullable: true),
                    SizeLength = table.Column<int>(type: "int", nullable: true),
                    SizeWidth = table.Column<int>(type: "int", nullable: true),
                    SizeHeight = table.Column<int>(type: "int", nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: true),
                    PsuReccomended = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gpus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hdds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Interface = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FormFactor = table.Column<double>(type: "float", nullable: true),
                    SpindleSpeed = table.Column<int>(type: "int", nullable: true),
                    Cache = table.Column<int>(type: "int", nullable: true),
                    Speed = table.Column<int>(type: "int", nullable: true),
                    WritingTechnology = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AverageLifetime = table.Column<int>(type: "int", nullable: true),
                    NoiceDb = table.Column<int>(type: "int", nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hdds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Motherboards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Socket = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Chipset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DimmSlots = table.Column<int>(type: "int", nullable: true),
                    DimmType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DimmFrequency = table.Column<int>(type: "int", nullable: true),
                    DimmCapacity = table.Column<int>(type: "int", nullable: true),
                    Sata3Count = table.Column<int>(type: "int", nullable: true),
                    PowerMotherboard = table.Column<int>(type: "int", nullable: true),
                    FanQuantity = table.Column<int>(type: "int", nullable: true),
                    Ethernet = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Audio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Wifi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Bluetooth = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VideoPorts = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FormFactor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SizeDimentions = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motherboards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PowerSupplies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FormFactor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Wattage = table.Column<int>(type: "int", nullable: false),
                    MolexCount = table.Column<int>(type: "int", nullable: true),
                    SataCount = table.Column<int>(type: "int", nullable: true),
                    FddCount = table.Column<int>(type: "int", nullable: true),
                    InputMinVoltage = table.Column<int>(type: "int", nullable: true),
                    InputMaxVoltage = table.Column<int>(type: "int", nullable: true),
                    HasApcf = table.Column<bool>(type: "bit", nullable: false),
                    EfficiencyStandart = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EfficiencyPercent = table.Column<double>(type: "float", nullable: true),
                    IsModular = table.Column<bool>(type: "bit", nullable: true),
                    NoiseLevelMaxDb = table.Column<int>(type: "int", nullable: true),
                    Size = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerSupplies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    ModuleQuantity = table.Column<int>(type: "int", nullable: true),
                    Timings = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Voltage = table.Column<double>(type: "float", nullable: true),
                    Xmp = table.Column<bool>(type: "bit", nullable: false),
                    Ecc = table.Column<bool>(type: "bit", nullable: false),
                    Expo = table.Column<bool>(type: "bit", nullable: false),
                    Bufferization = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ssds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Interface = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NandType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsTrimmSupported = table.Column<bool>(type: "bit", nullable: false),
                    FormFactor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Size = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    MaxReadSpeed = table.Column<int>(type: "int", nullable: true),
                    MaxWriteSpeed = table.Column<int>(type: "int", nullable: true),
                    RandomReadSpeed = table.Column<int>(type: "int", nullable: true),
                    RandomWriteSpeed = table.Column<int>(type: "int", nullable: true),
                    WritingRecource = table.Column<int>(type: "int", nullable: true),
                    AverageLifeTime = table.Column<double>(type: "float", nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: false),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ssds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CpuCoolerSockets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SocketType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CpuCoolerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CpuCoolerSockets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CpuCoolerSockets_CpuCoolers_CpuCoolerId",
                        column: x => x.CpuCoolerId,
                        principalTable: "CpuCoolers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Case_FanLocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FanSize = table.Column<int>(type: "int", nullable: false),
                    MaxFans = table.Column<int>(type: "int", nullable: false),
                    CaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FanLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "GpuPowerConnectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Pins = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    GpuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GpuPowerConnectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GpuPowerConnectors_Gpus_GpuId",
                        column: x => x.GpuId,
                        principalTable: "Gpus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CpuPowerConnectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Pins = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    MotherboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CpuPowerConnectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CpuPowerConnectors_Motherboards_MotherboardId",
                        column: x => x.MotherboardId,
                        principalTable: "Motherboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InnerPorts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MotherboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InnerPorts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InnerPorts_Motherboards_MotherboardId",
                        column: x => x.MotherboardId,
                        principalTable: "Motherboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "M2Slots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<double>(type: "float", nullable: false),
                    Lane = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    MotherboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_M2Slots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_M2Slots_Motherboards_MotherboardId",
                        column: x => x.MotherboardId,
                        principalTable: "Motherboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PcleSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<double>(type: "float", nullable: false),
                    Lane = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    MotherboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcleSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcleSlots_Motherboards_MotherboardId",
                        column: x => x.MotherboardId,
                        principalTable: "Motherboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RearPorts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    MotherboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RearPorts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RearPorts_Motherboards_MotherboardId",
                        column: x => x.MotherboardId,
                        principalTable: "Motherboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PowerSupplyPowerConnectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Pins = table.Column<int>(type: "int", nullable: false),
                    AdditionalPins = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PowerSupplyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerSupplyPowerConnectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerSupplyPowerConnectors_PowerSupplies_PowerSupplyId",
                        column: x => x.PowerSupplyId,
                        principalTable: "PowerSupplies",
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

            migrationBuilder.CreateIndex(
                name: "IX_CpuCoolerSockets_CpuCoolerId",
                table: "CpuCoolerSockets",
                column: "CpuCoolerId");

            migrationBuilder.CreateIndex(
                name: "IX_CpuPowerConnectors_MotherboardId",
                table: "CpuPowerConnectors",
                column: "MotherboardId");

            migrationBuilder.CreateIndex(
                name: "IX_GpuPowerConnectors_GpuId",
                table: "GpuPowerConnectors",
                column: "GpuId");

            migrationBuilder.CreateIndex(
                name: "IX_InnerPorts_MotherboardId",
                table: "InnerPorts",
                column: "MotherboardId");

            migrationBuilder.CreateIndex(
                name: "IX_M2Slots_MotherboardId",
                table: "M2Slots",
                column: "MotherboardId");

            migrationBuilder.CreateIndex(
                name: "IX_PcleSlots_MotherboardId",
                table: "PcleSlots",
                column: "MotherboardId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerSupplyPowerConnectors_PowerSupplyId",
                table: "PowerSupplyPowerConnectors",
                column: "PowerSupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_RearPorts_MotherboardId",
                table: "RearPorts",
                column: "MotherboardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Case_FanLocation");

            migrationBuilder.DropTable(
                name: "Case_FormFactor");

            migrationBuilder.DropTable(
                name: "CpuCoolerSockets");

            migrationBuilder.DropTable(
                name: "CpuPowerConnectors");

            migrationBuilder.DropTable(
                name: "Cpus");

            migrationBuilder.DropTable(
                name: "Fans");

            migrationBuilder.DropTable(
                name: "GpuPowerConnectors");

            migrationBuilder.DropTable(
                name: "Hdds");

            migrationBuilder.DropTable(
                name: "InnerPorts");

            migrationBuilder.DropTable(
                name: "M2Slots");

            migrationBuilder.DropTable(
                name: "PcleSlots");

            migrationBuilder.DropTable(
                name: "PowerSupplyPowerConnectors");

            migrationBuilder.DropTable(
                name: "Rams");

            migrationBuilder.DropTable(
                name: "RearPorts");

            migrationBuilder.DropTable(
                name: "Ssds");

            migrationBuilder.DropTable(
                name: "FanLocations");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "FormFactors");

            migrationBuilder.DropTable(
                name: "CpuCoolers");

            migrationBuilder.DropTable(
                name: "Gpus");

            migrationBuilder.DropTable(
                name: "PowerSupplies");

            migrationBuilder.DropTable(
                name: "Motherboards");
        }
    }
}
