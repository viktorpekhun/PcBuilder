using Microsoft.EntityFrameworkCore;
using PcBuilderApi.Models;
namespace PcBuilderApi.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option) :base(option) { }

        public DbSet<Cpu> Cpus { get; set; }
        public DbSet<Gpu> Gpus { get; set; }
        public DbSet<Motherboard> Motherboards { get; set; }
        public DbSet<CpuPowerConnector> CpuPowerConnectors { get; set; }
        public DbSet<GpuPowerConnector> GpuPowerConnectors { get; set; }
        public DbSet<PcleSlot> PcleSlots { get; set; }
        public DbSet<M2Slot> M2Slots { get; set; }
        public DbSet<RearPort> RearPorts { get; set; }
        public DbSet<InnerPort> InnerPorts { get; set; }
        public DbSet<CpuCooler> CpuCoolers { get; set; }
        public DbSet<CpuCoolerSocket> CpuCoolerSockets { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<FormFactor> FormFactors { get; set; }
        public DbSet<Case_FormFactor> Case_FormFactors { get; set; }
        public DbSet<FanLocation> FanLocations { get; set; }
        public DbSet<Case_FanLocation> Case_FanLocations { get; set; }
        public DbSet<PowerSupply> PowerSupplies { get; set; }
        public DbSet<PowerSupplyPowerConnector> PowerSupplyPowerConnectors { get; set; }
        public DbSet<Ram> Rams { get; set; }
        public DbSet<Ssd> Ssds { get; set; }
        public DbSet<Hdd> Hdds { get; set; }
        public DbSet<Fan> Fans { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<ProductOffer> ProductOffers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CpuPowerConnector>()
                .HasOne(c => c.Motherboard)
                .WithMany(m => m.CpuPowerConnectors)
                .HasForeignKey(c => c.MotherboardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GpuPowerConnector>()
                .HasOne(c => c.Gpu)
                .WithMany(m => m.GpuPowerConnectors)
                .HasForeignKey(c => c.GpuId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PcleSlot>()
                .HasOne(c => c.Motherboard)
                .WithMany(m => m.PcleSlots)
                .HasForeignKey(c => c.MotherboardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<M2Slot>()
                .HasOne(c => c.Motherboard)
                .WithMany(m => m.M2Slots)
                .HasForeignKey(c => c.MotherboardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RearPort>()
                .HasOne(c => c.Motherboard)
                .WithMany(m => m.RearPorts)
                .HasForeignKey(c => c.MotherboardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InnerPort>()
                .HasOne(c => c.Motherboard)
                .WithMany(m => m.InnerPorts)
                .HasForeignKey(c => c.MotherboardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CpuCooler>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CHK_CpuCooler_Type", "Type IN ('Air', 'Water')"));
            });

            modelBuilder.Entity<CpuCoolerSocket>()
                .HasOne(c => c.CpuCooler)
                .WithMany(m => m.CpuCoolerSockets)
                .HasForeignKey(c => c.CpuCoolerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Case_FormFactor>()
                .ToTable("Case_FormFactor")
                .HasOne(cf => cf.Case)
                .WithMany(c => c.Case_FormFactors)
                .HasForeignKey(cf => cf.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Case_FormFactor>()
                .HasOne(cf => cf.FormFactor)
                .WithMany(f => f.Case_FormFactors)
                .HasForeignKey(cf => cf.FormFactorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Case_FanLocation>()
                .ToTable("Case_FanLocation")
                .HasOne(cf => cf.Case)
                .WithMany(c => c.Case_FanLocations)
                .HasForeignKey(cf => cf.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Case_FanLocation>()
                .HasOne(cf => cf.FanLocation)
                .WithMany(f => f.Case_FanLocations)
                .HasForeignKey(cf => cf.FanLocationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PowerSupplyPowerConnector>()
                .HasOne(c => c.PowerSupply)
                .WithMany(m => m.PowerSupplyPowerConnectors)
                .HasForeignKey(cf => cf.PowerSupplyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Store>()
                .HasMany(s => s.ProductOffers)
                .WithOne(po => po.Store)
                .HasForeignKey(po => po.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductOffer>()
                .HasIndex(po => new { po.ComponentId, po.ComponentType });
        }
    }
}
