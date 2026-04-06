using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Persistence;
using Components.Domain.Entities;
using Components.Domain.ValueObjects;
using Auth.Domain.Entities;
using PcBuilds.Domain.Entities;

namespace PcBuilder.Persistence.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option) : base(option) { }

        // Components
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
        public DbSet<PcCase> PcCases { get; set; }
        public DbSet<PcCaseFormFactor> PcCaseFormFactors { get; set; }
        public DbSet<PcCaseFanLocation> PcCaseFanLocations { get; set; }
        public DbSet<PowerSupply> PowerSupplies { get; set; }
        public DbSet<PowerSupplyPowerConnector> PowerSupplyPowerConnectors { get; set; }
        public DbSet<Ram> Rams { get; set; }
        public DbSet<Ssd> Ssds { get; set; }
        public DbSet<Hdd> Hdds { get; set; }
        public DbSet<Fan> Fans { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<ProductOffer> ProductOffers { get; set; }

        // Auth
        public DbSet<User> Users { get; set; }

        // PcBuilds
        public DbSet<PcBuild> PcBuilds { get; set; }
        public DbSet<PcBuild_Ssd> PcBuild_Ssds { get; set; }
        public DbSet<PcBuild_Hdd> PcBuild_Hdds { get; set; }
        public DbSet<PcBuild_Ram> PcBuild_Rams { get; set; }
        public DbSet<PcBuild_Fan> PcBuild_Fans { get; set; }
        public DbSet<Review> Reviews { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Component child entity relationships
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

            modelBuilder.Entity<PcCaseFormFactor>()
                .HasOne(cf => cf.PcCase)
                .WithMany(c => c.PcCaseFormFactors)
                .HasForeignKey(cf => cf.PcCaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PcCaseFanLocation>(entity =>
            {
                entity.HasOne(cf => cf.PcCase)
                    .WithMany(c => c.PcCaseFanLocations)
                    .HasForeignKey(cf => cf.PcCaseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.OwnsOne(p => p.Name, b =>
                {
                    b.Property(l => l.Uk).HasColumnName("Name_Uk").HasMaxLength(50).IsRequired();
                    b.Property(l => l.En).HasColumnName("Name_En").HasMaxLength(50);
                });
            });

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

            // PcBuild relationships (cross-module nav props removed from Component/User side)
            modelBuilder.Entity<PcBuild>()
                .HasOne(pb => pb.User)
                .WithMany()
                .HasForeignKey(pb => pb.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PcBuild_Ssd>()
                .ToTable("PcBuild_Ssd")
                .HasOne(ps => ps.PcBuild)
                .WithMany(pb => pb.PcBuild_Ssds)
                .HasForeignKey(ps => ps.PcBuildId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PcBuild_Ssd>()
                .HasOne(ps => ps.Ssd)
                .WithMany()
                .HasForeignKey(ps => ps.SsdId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PcBuild_Hdd>()
                .ToTable("PcBuild_Hdd")
                .HasOne(ps => ps.PcBuild)
                .WithMany(pb => pb.PcBuild_Hdds)
                .HasForeignKey(ps => ps.PcBuildId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PcBuild_Hdd>()
                .HasOne(ps => ps.Hdd)
                .WithMany()
                .HasForeignKey(ps => ps.HddId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PcBuild_Ram>()
                .ToTable("PcBuild_Ram")
                .HasOne(ps => ps.PcBuild)
                .WithMany(pb => pb.PcBuild_Rams)
                .HasForeignKey(ps => ps.PcBuildId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PcBuild_Ram>()
                .HasOne(ps => ps.Ram)
                .WithMany()
                .HasForeignKey(ps => ps.RamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PcBuild_Fan>()
                .ToTable("PcBuild_Fan")
                .HasOne(ps => ps.PcBuild)
                .WithMany(pb => pb.PcBuild_Fans)
                .HasForeignKey(ps => ps.PcBuildId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PcBuild_Fan>()
                .HasOne(ps => ps.Fan)
                .WithMany()
                .HasForeignKey(ps => ps.FanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.PcBuild)
                .WithMany(pb => pb.Reviews)
                .HasForeignKey(r => r.PcBuildId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<Review>()
                .Property(u => u.Rating)
                .HasPrecision(3, 2);

            modelBuilder.Entity<PcBuild>()
                .Property(u => u.AverageRating)
                .HasPrecision(3, 2);

            modelBuilder.Entity<PcBuild>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductOffer>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);


            modelBuilder.Entity<Cpu>()
                .Property(p => p.AveragePrice)
                .HasPrecision(18, 2);
            //modelBuilder.Entity<Cpu>()
            //    .OwnsOne(p => p.Description, builder =>
            //    {
            //        builder.ToJson();
            //    });


            modelBuilder.Entity<Gpu>()
                .Property(p => p.AveragePrice)
                .HasPrecision(18, 2);


            modelBuilder.Entity<Ram>(entity =>
            {
                entity.Property(p => p.AveragePrice).HasPrecision(18, 2);

                entity.OwnsOne(p => p.Color, b =>
                {
                    b.Property(l => l.Uk).HasColumnName("Color_Uk").HasMaxLength(100).IsRequired();
                    b.Property(l => l.En).HasColumnName("Color_En").HasMaxLength(100);
                });
            });


            modelBuilder.Entity<Motherboard>()
                .Property(p => p.AveragePrice)
                .HasPrecision(18, 2);


            modelBuilder.Entity<CpuCooler>(entity =>
            {
                entity.Property(p => p.AveragePrice).HasPrecision(18, 2);

                entity.OwnsOne(p => p.RadiatorMaterial, b =>
                {
                    b.Property(l => l.Uk).HasColumnName("RadiatorMaterial_Uk").HasMaxLength(100).IsRequired();
                    b.Property(l => l.En).HasColumnName("RadiatorMaterial_En").HasMaxLength(100);
                });
            });


            modelBuilder.Entity<PcCase>(entity =>
            {
                entity.Property(p => p.AveragePrice).HasPrecision(18, 2);

                entity.OwnsOne(p => p.PsuLocation, b =>
                {
                    b.Property(l => l.Uk).HasColumnName("PsuLocation_Uk").HasMaxLength(100).IsRequired();
                    b.Property(l => l.En).HasColumnName("PsuLocation_En").HasMaxLength(100);
                });

                entity.OwnsOne(p => p.BuiltInFans, b =>
                {
                    b.Property(l => l.Uk).HasColumnName("BuiltInFans_Uk").HasMaxLength(200).IsRequired();
                    b.Property(l => l.En).HasColumnName("BuiltInFans_En").HasMaxLength(200);
                });

                entity.OwnsOne(p => p.AdditionalFanPlaces, b =>
                {
                    b.Property(l => l.Uk).HasColumnName("AdditionalFanPlaces_Uk").HasMaxLength(200).IsRequired();
                    b.Property(l => l.En).HasColumnName("AdditionalFanPlaces_En").HasMaxLength(200);
                });
            });


            modelBuilder.Entity<PowerSupply>(entity =>
            {
                entity.Property(p => p.AveragePrice).HasPrecision(18, 2);

                entity.OwnsOne(p => p.Modularity, b =>
                {
                    b.Property(l => l.Uk).HasColumnName("Modularity_Uk").HasMaxLength(100).IsRequired();
                    b.Property(l => l.En).HasColumnName("Modularity_En").HasMaxLength(100);
                });
            });


            modelBuilder.Entity<Ssd>()
                .Property(p => p.AveragePrice)
                .HasPrecision(18, 2);


            modelBuilder.Entity<Hdd>()
                .Property(p => p.AveragePrice)
                .HasPrecision(18, 2);


            modelBuilder.Entity<Fan>(entity =>
            {
                entity.Property(p => p.AveragePrice).HasPrecision(18, 2);

                entity.OwnsOne(p => p.BearingType, b =>
                {
                    b.Property(l => l.Uk).HasColumnName("BearingType_Uk").HasMaxLength(100).IsRequired();
                    b.Property(l => l.En).HasColumnName("BearingType_En").HasMaxLength(100);
                });

                entity.OwnsOne(p => p.Color, b =>
                {
                    b.Property(l => l.Uk).HasColumnName("Color_Uk").HasMaxLength(100).IsRequired();
                    b.Property(l => l.En).HasColumnName("Color_En").HasMaxLength(100);
                });
            });
                

        }
    }
}
