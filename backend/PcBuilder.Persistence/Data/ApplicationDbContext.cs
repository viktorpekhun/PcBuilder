using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Persistence;
using Components.Domain.Entities;
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
        public DbSet<Role> Roles { get; set; }

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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
