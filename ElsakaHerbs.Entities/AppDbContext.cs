using ElsakaHerbs.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElsakaHerbs.DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SiteContent> SiteContents => Set<SiteContent>();
    public DbSet<GovernorateShipping> GovernorateShippings => Set<GovernorateShipping>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<PackageImage> PackageImages => Set<PackageImage>();
    public DbSet<CustomerReviewImage> CustomerReviewImages => Set<CustomerReviewImage>();
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── SiteContent singleton enforcement ────────────────────────────────
        modelBuilder.Entity<SiteContent>(b =>
        {
            b.Property<byte>("SingletonEnforcer")
             .HasDefaultValue((byte)1);

            b.ToTable(t => t.HasCheckConstraint(
                "CK_SiteContent_Singleton",
                "[SingletonEnforcer] = 1"));

            b.HasIndex("SingletonEnforcer").IsUnique();

            b.Property(sc => sc.HeroTitle).HasMaxLength(200).IsRequired();
            b.Property(sc => sc.HeroHighlight).HasMaxLength(200).IsRequired();
            b.Property(sc => sc.CtaEyebrow).HasMaxLength(150).IsRequired();
            b.Property(sc => sc.CtaTitle).HasMaxLength(200).IsRequired();
            b.Property(sc => sc.CtaDescription).IsRequired();
            b.Property(sc => sc.WhatsAppNumber).HasMaxLength(30).IsRequired();
            b.Property(sc => sc.IsWebsiteEnabled).HasDefaultValue(true);
            b.Property(sc => sc.MaintenanceMessage).HasMaxLength(1000);
        });

        // ── GovernorateShipping ──────────────────────────────────────────────
        modelBuilder.Entity<GovernorateShipping>(b =>
        {
            b.HasIndex(g => g.GovernorateKey).IsUnique();
            b.Property(g => g.GovernorateKey).HasMaxLength(50).IsRequired();
            b.Property(g => g.GovernorateName).HasMaxLength(100).IsRequired();
            b.Property(g => g.ShippingCost).HasColumnType("decimal(18,2)");
        });

        // ── Package ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Package>(b =>
        {
            b.Property(p => p.Name).HasMaxLength(200).IsRequired();
            b.Property(p => p.Description).IsRequired();
            b.Property(p => p.Price).HasColumnType("decimal(18,2)");
            b.Property(p => p.OriginalPrice).HasColumnType("decimal(18,2)");
        });

        // ── PackageImage ─────────────────────────────────────────────────────
        modelBuilder.Entity<PackageImage>(b =>
        {
            b.Property(pi => pi.ImageUrl).HasMaxLength(500).IsRequired();

            // Filtered unique index: at most one default image per package
            b.HasIndex(pi => new { pi.PackageId, pi.IsDefault })
             .IsUnique()
             .HasFilter("[IsDefault] = 1");

            b.HasOne(pi => pi.Package)
             .WithMany(p => p.Images)
             .HasForeignKey(pi => pi.PackageId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── CustomerReviewImage ──────────────────────────────────────────────
        modelBuilder.Entity<CustomerReviewImage>(b =>
        {
            b.Property(r => r.ImageUrl).HasMaxLength(500).IsRequired();
        });

        // ── OrderStatus ──────────────────────────────────────────────────────
        modelBuilder.Entity<OrderStatus>(b =>
        {
            b.HasIndex(s => s.StatusName).IsUnique();
            b.Property(s => s.StatusName).HasMaxLength(50).IsRequired();
            b.Property(s => s.StatusColor).HasMaxLength(50).IsRequired();

            b.HasData(
                new OrderStatus { Id = 1, StatusName = "طلب جديد", StatusColor = "#0d6efd" },
                new OrderStatus { Id = 2, StatusName = "تم الشحن", StatusColor = "#f59e0b" },
                new OrderStatus { Id = 3, StatusName = "تم التسليم", StatusColor = "#10b981" },
                new OrderStatus { Id = 4, StatusName = "ملغي", StatusColor = "#ef4444" }
            );
        });

        // ── Order ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Order>(b =>
        {
            b.HasIndex(o => o.OrderNumber).IsUnique();
            b.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            b.Property(o => o.CustomerFirstName).HasMaxLength(100).IsRequired();
            b.Property(o => o.CustomerLastName).HasMaxLength(100).IsRequired();
            b.Property(o => o.CustomerPhone).HasMaxLength(30).IsRequired();
            b.Property(o => o.GovernorateNameSnapshot).HasMaxLength(100).IsRequired();
            b.Property(o => o.Address).IsRequired();
            b.Property(o => o.Subtotal).HasColumnType("decimal(18,2)");
            b.Property(o => o.ShippingCost).HasColumnType("decimal(18,2)");
            b.Property(o => o.GrandTotal).HasColumnType("decimal(18,2)");

            b.HasOne(o => o.GovernorateShipping)
             .WithMany(g => g.Orders)
             .HasForeignKey(o => o.GovernorateShippingId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(o => o.OrderStatus)
             .WithMany(s => s.Orders)
             .HasForeignKey(o => o.OrderStatusId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── OrderItem ────────────────────────────────────────────────────────
        modelBuilder.Entity<OrderItem>(b =>
        {
            b.Property(oi => oi.PackageNameSnapshot).HasMaxLength(200).IsRequired();
            b.Property(oi => oi.UnitPriceSnapshot).HasColumnType("decimal(18,2)");
            b.Property(oi => oi.LineTotal).HasColumnType("decimal(18,2)");

            b.HasOne(oi => oi.Order)
             .WithMany(o => o.OrderItems)
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            // Nullable FK — SetNull when a Package is hard-deleted
            b.HasOne(oi => oi.Package)
             .WithMany(p => p.OrderItems)
             .HasForeignKey(oi => oi.PackageId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
