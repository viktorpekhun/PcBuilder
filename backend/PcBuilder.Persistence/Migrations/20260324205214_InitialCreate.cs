using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PcBuilder.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CpuCoolers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FanCount = table.Column<int>(type: "int", nullable: true),
                    FanSize = table.Column<double>(type: "float", nullable: true),
                    RadiatorMaterial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SpeedControl = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PowerConnector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaxPowerDissipation = table.Column<int>(type: "int", nullable: true),
                    MaxSpeed = table.Column<int>(type: "int", nullable: true),
                    MinSpeed = table.Column<int>(type: "int", nullable: true),
                    AirflowCfm = table.Column<double>(type: "float", nullable: true),
                    NoiseLevelDb = table.Column<double>(type: "float", nullable: true),
                    Voltage = table.Column<int>(type: "int", nullable: true),
                    Lifespan = table.Column<int>(type: "int", nullable: true),
                    Length = table.Column<double>(type: "float", nullable: true),
                    Width = table.Column<double>(type: "float", nullable: true),
                    Height = table.Column<double>(type: "float", nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AveragePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OffersCount = table.Column<int>(type: "int", nullable: true)
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
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Socket = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BasicFrequency = table.Column<double>(type: "float", nullable: true),
                    MaxFrequency = table.Column<double>(type: "float", nullable: true),
                    Cache = table.Column<int>(type: "int", nullable: true),
                    DimmType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Cores = table.Column<int>(type: "int", nullable: true),
                    Threads = table.Column<int>(type: "int", nullable: true),
                    Techprocess = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tdp = table.Column<int>(type: "int", nullable: true),
                    IntegratedGraphics = table.Column<bool>(type: "bit", nullable: false),
                    Complectation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AveragePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OffersCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cpus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModuleCount = table.Column<int>(type: "int", nullable: true),
                    BearingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SpeedControl = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Connector = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MinSpeed = table.Column<int>(type: "int", nullable: true),
                    MaxSpeed = table.Column<int>(type: "int", nullable: true),
                    AirflowCfm = table.Column<double>(type: "float", nullable: true),
                    NoiseLevelDb = table.Column<double>(type: "float", nullable: true),
                    Voltage = table.Column<int>(type: "int", nullable: true),
                    SizeLength = table.Column<double>(type: "float", nullable: true),
                    SizeWidth = table.Column<double>(type: "float", nullable: true),
                    SizeHeight = table.Column<double>(type: "float", nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AveragePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OffersCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gpus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GpuManufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GpuModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Memory = table.Column<int>(type: "int", nullable: true),
                    MemoryType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PcleVersion = table.Column<double>(type: "float", nullable: true),
                    PcleLane = table.Column<int>(type: "int", nullable: true),
                    MaxFrequency = table.Column<int>(type: "int", nullable: true),
                    CudaCores = table.Column<int>(type: "int", nullable: true),
                    MemorySpeed = table.Column<int>(type: "int", nullable: true),
                    MemoryBus = table.Column<int>(type: "int", nullable: true),
                    SizeLength = table.Column<double>(type: "float", nullable: true),
                    SizeWidth = table.Column<double>(type: "float", nullable: true),
                    SizeHeight = table.Column<double>(type: "float", nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: true),
                    PsuReccomended = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AveragePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OffersCount = table.Column<int>(type: "int", nullable: true)
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
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Interface = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FormFactor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpindleSpeed = table.Column<int>(type: "int", nullable: true),
                    Cache = table.Column<int>(type: "int", nullable: true),
                    Speed = table.Column<int>(type: "int", nullable: true),
                    WritingTechnology = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NoiceDb = table.Column<int>(type: "int", nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AveragePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OffersCount = table.Column<int>(type: "int", nullable: true)
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
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
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
                    PcleX1Quantity = table.Column<int>(type: "int", nullable: true),
                    Ethernet = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Audio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Wifi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Bluetooth = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VideoPorts = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FormFactor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SizeDimentions = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AveragePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OffersCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motherboards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PcCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeStandard = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SizeDimentions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    PsuWattage = table.Column<int>(type: "int", nullable: true),
                    PsuLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaxGpuLength = table.Column<double>(type: "float", nullable: true),
                    MaxCpuCoolerHeight = table.Column<double>(type: "float", nullable: true),
                    HasDustFilters = table.Column<bool>(type: "bit", nullable: false),
                    BuiltInFans = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AdditionalFanPlaces = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Slot25Quant = table.Column<int>(type: "int", nullable: true),
                    Slot35Quant = table.Column<int>(type: "int", nullable: true),
                    Slot525Quant = table.Column<int>(type: "int", nullable: true),
                    ExpansionSlotQuant = table.Column<int>(type: "int", nullable: true),
                    Usb = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HasHeadphones = table.Column<bool>(type: "bit", nullable: false),
                    HasMicrophone = table.Column<bool>(type: "bit", nullable: false),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AveragePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OffersCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcCases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PowerSupplies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FormFactor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Wattage = table.Column<int>(type: "int", nullable: false),
                    MolexCount = table.Column<int>(type: "int", nullable: true),
                    SataCount = table.Column<int>(type: "int", nullable: true),
                    FddCount = table.Column<int>(type: "int", nullable: true),
                    InputMinVoltage = table.Column<int>(type: "int", nullable: true),
                    InputMaxVoltage = table.Column<int>(type: "int", nullable: true),
                    HasApcf = table.Column<bool>(type: "bit", nullable: false),
                    EfficiencyStandart = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EfficiencyPercent = table.Column<double>(type: "float", nullable: true),
                    IsModular = table.Column<bool>(type: "bit", nullable: false),
                    NoiseLevelMaxDb = table.Column<double>(type: "float", nullable: true),
                    Size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AveragePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OffersCount = table.Column<int>(type: "int", nullable: true)
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
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    ModuleQuantity = table.Column<int>(type: "int", nullable: true),
                    Timings = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Voltage = table.Column<double>(type: "float", nullable: true),
                    Xmp = table.Column<bool>(type: "bit", nullable: false),
                    Ecc = table.Column<bool>(type: "bit", nullable: false),
                    Expo = table.Column<bool>(type: "bit", nullable: false),
                    Bufferization = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: true),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AveragePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OffersCount = table.Column<int>(type: "int", nullable: true)
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
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Interface = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NandType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsTrimmSupported = table.Column<bool>(type: "bit", nullable: false),
                    FormFactor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    MaxReadSpeed = table.Column<int>(type: "int", nullable: true),
                    MaxWriteSpeed = table.Column<int>(type: "int", nullable: true),
                    RandomReadSpeed = table.Column<int>(type: "int", nullable: true),
                    RandomWriteSpeed = table.Column<int>(type: "int", nullable: true),
                    WritingRecource = table.Column<int>(type: "int", nullable: true),
                    AverageLifeTime = table.Column<double>(type: "float", nullable: true),
                    Wattage = table.Column<int>(type: "int", nullable: false),
                    FactoryLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AveragePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OffersCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ssds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Likes = table.Column<int>(type: "int", nullable: false),
                    Dislikes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "PowerSupplyPowerConnectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "ProductOffers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ComponentType = table.Column<int>(type: "int", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductOfferUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductOffers_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PcBuilds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CpuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CpuOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GpuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GpuOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MotherboardId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MotherboardOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CpuCoolerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CpuCoolerOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PowerSupplyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PowerSupplyOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PcCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PcCaseOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PcBuilds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PcBuilds_CpuCoolers_CpuCoolerId",
                        column: x => x.CpuCoolerId,
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
                        name: "FK_PcBuilds_ProductOffers_CpuCoolerOfferId",
                        column: x => x.CpuCoolerOfferId,
                        principalTable: "ProductOffers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcBuilds_ProductOffers_CpuOfferId",
                        column: x => x.CpuOfferId,
                        principalTable: "ProductOffers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcBuilds_ProductOffers_GpuOfferId",
                        column: x => x.GpuOfferId,
                        principalTable: "ProductOffers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcBuilds_ProductOffers_MotherboardOfferId",
                        column: x => x.MotherboardOfferId,
                        principalTable: "ProductOffers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcBuilds_ProductOffers_PcCaseOfferId",
                        column: x => x.PcCaseOfferId,
                        principalTable: "ProductOffers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PcBuilds_ProductOffers_PowerSupplyOfferId",
                        column: x => x.PowerSupplyOfferId,
                        principalTable: "ProductOffers",
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
                    ProductOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_PcBuild_Fan_ProductOffers_ProductOfferId",
                        column: x => x.ProductOfferId,
                        principalTable: "ProductOffers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PcBuild_Hdd",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    HddId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_PcBuild_Hdd_ProductOffers_ProductOfferId",
                        column: x => x.ProductOfferId,
                        principalTable: "ProductOffers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PcBuild_Ram",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    RamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                        name: "FK_PcBuild_Ram_ProductOffers_ProductOfferId",
                        column: x => x.ProductOfferId,
                        principalTable: "ProductOffers",
                        principalColumn: "Id");
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
                    ProductOfferId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                        name: "FK_PcBuild_Ssd_ProductOffers_ProductOfferId",
                        column: x => x.ProductOfferId,
                        principalTable: "ProductOffers",
                        principalColumn: "Id");
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
                name: "IX_PcBuild_Fan_FanId",
                table: "PcBuild_Fan",
                column: "FanId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Fan_PcBuildId",
                table: "PcBuild_Fan",
                column: "PcBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Fan_ProductOfferId",
                table: "PcBuild_Fan",
                column: "ProductOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Hdd_HddId",
                table: "PcBuild_Hdd",
                column: "HddId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Hdd_PcBuildId",
                table: "PcBuild_Hdd",
                column: "PcBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Hdd_ProductOfferId",
                table: "PcBuild_Hdd",
                column: "ProductOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Ram_PcBuildId",
                table: "PcBuild_Ram",
                column: "PcBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Ram_ProductOfferId",
                table: "PcBuild_Ram",
                column: "ProductOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Ram_RamId",
                table: "PcBuild_Ram",
                column: "RamId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Ssd_PcBuildId",
                table: "PcBuild_Ssd",
                column: "PcBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Ssd_ProductOfferId",
                table: "PcBuild_Ssd",
                column: "ProductOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuild_Ssd_SsdId",
                table: "PcBuild_Ssd",
                column: "SsdId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_CpuCoolerId",
                table: "PcBuilds",
                column: "CpuCoolerId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_CpuCoolerOfferId",
                table: "PcBuilds",
                column: "CpuCoolerOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_CpuId",
                table: "PcBuilds",
                column: "CpuId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_CpuOfferId",
                table: "PcBuilds",
                column: "CpuOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_GpuId",
                table: "PcBuilds",
                column: "GpuId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_GpuOfferId",
                table: "PcBuilds",
                column: "GpuOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_MotherboardId",
                table: "PcBuilds",
                column: "MotherboardId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_MotherboardOfferId",
                table: "PcBuilds",
                column: "MotherboardOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_PcCaseId",
                table: "PcBuilds",
                column: "PcCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_PcCaseOfferId",
                table: "PcBuilds",
                column: "PcCaseOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_PowerSupplyId",
                table: "PcBuilds",
                column: "PowerSupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_PowerSupplyOfferId",
                table: "PcBuilds",
                column: "PowerSupplyOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_PcBuilds_UserId",
                table: "PcBuilds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PcCaseFanLocations_PcCaseId",
                table: "PcCaseFanLocations",
                column: "PcCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PcCaseFormFactors_PcCaseId",
                table: "PcCaseFormFactors",
                column: "PcCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PcleSlots_MotherboardId",
                table: "PcleSlots",
                column: "MotherboardId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerSupplyPowerConnectors_PowerSupplyId",
                table: "PowerSupplyPowerConnectors",
                column: "PowerSupplyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOffers_ComponentId_ComponentType",
                table: "ProductOffers",
                columns: new[] { "ComponentId", "ComponentType" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductOffers_StoreId",
                table: "ProductOffers",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_RearPorts_MotherboardId",
                table: "RearPorts",
                column: "MotherboardId");

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
                name: "CpuCoolerSockets");

            migrationBuilder.DropTable(
                name: "CpuPowerConnectors");

            migrationBuilder.DropTable(
                name: "GpuPowerConnectors");

            migrationBuilder.DropTable(
                name: "InnerPorts");

            migrationBuilder.DropTable(
                name: "M2Slots");

            migrationBuilder.DropTable(
                name: "PcBuild_Fan");

            migrationBuilder.DropTable(
                name: "PcBuild_Hdd");

            migrationBuilder.DropTable(
                name: "PcBuild_Ram");

            migrationBuilder.DropTable(
                name: "PcBuild_Ssd");

            migrationBuilder.DropTable(
                name: "PcCaseFanLocations");

            migrationBuilder.DropTable(
                name: "PcCaseFormFactors");

            migrationBuilder.DropTable(
                name: "PcleSlots");

            migrationBuilder.DropTable(
                name: "PowerSupplyPowerConnectors");

            migrationBuilder.DropTable(
                name: "RearPorts");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Fans");

            migrationBuilder.DropTable(
                name: "Hdds");

            migrationBuilder.DropTable(
                name: "Rams");

            migrationBuilder.DropTable(
                name: "Ssds");

            migrationBuilder.DropTable(
                name: "PcBuilds");

            migrationBuilder.DropTable(
                name: "CpuCoolers");

            migrationBuilder.DropTable(
                name: "Cpus");

            migrationBuilder.DropTable(
                name: "Gpus");

            migrationBuilder.DropTable(
                name: "Motherboards");

            migrationBuilder.DropTable(
                name: "PcCases");

            migrationBuilder.DropTable(
                name: "PowerSupplies");

            migrationBuilder.DropTable(
                name: "ProductOffers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Stores");
        }
    }
}
