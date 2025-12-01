using KrollDiscounting.Entities;
using Microsoft.EntityFrameworkCore;

namespace KrollDiscounting.Data
{
    public class BondContext : DbContext
    {
        public BondContext(DbContextOptions<BondContext> options)
            : base(options)
        {
        }

        public DbSet<Bond> Bonds { get; set; } = null!;
        public DbSet<DiscountRateMethdologyAbstract> DiscountRateMethodologies { get; set; } = null!;
        public DbSet<BenchmarkSpreadAbstract> BenchmarkSpreads { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Bond>(eb =>
            {
                eb.HasKey(b => b.Id);
                // Database will generate the Id value on insert
                eb.Property(b => b.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<DiscountRateMethdologyAbstract>(ed =>
            {
                ed.HasKey(d => d.Id);
                // Database will generate the Id value on insert
                ed.Property(d => d.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<BenchmarkSpreadAbstract>(es =>
            {
                es.HasKey(s => s.Id);
                // Database will generate the Id value on insert
                es.Property(s => s.Id).ValueGeneratedOnAdd();
                // Table name is provided by [Table("BenchmarkSpreads")] on the type.
            });

            // If you want explicit configuration for concrete types (optional):
            modelBuilder.Entity<AvgBenchmarkSpread>();
            modelBuilder.Entity<MedianBenchmarkSpread>();

            modelBuilder.Entity<PriceBasedDiscountRateMethodology>();
            modelBuilder.Entity<YieldBasedDiscountRateMethodology>();

            modelBuilder.Entity<ZeroCouponCorporateBond>();
            modelBuilder.Entity<CouponCorporateBond>();

            // If there are relationships (e.g. YieldBasedDiscountRateMethodology -> BenchmarkSpread),
            // configure them here. Example (uncomment and adjust if needed):
            modelBuilder.Entity<YieldBasedDiscountRateMethodology>()
                 .HasOne(y => y.BenchmarkSpread)
                 .WithOne(p => p.DiscountRateMethodology)   //.WithMany()               // or .WithOne(...) if reverse nav exists
                 .HasForeignKey<BenchmarkSpreadAbstract>(b => b.DiscountRateMethdologyAbstractId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}