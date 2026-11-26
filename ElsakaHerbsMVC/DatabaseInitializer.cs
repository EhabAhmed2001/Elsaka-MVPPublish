using ElsakaHerbs.DataAccess;
using ElsakaHerbs.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElsakaHerbsMVC;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, IWebHostEnvironment environment)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // Apply any pending migrations
        await context.Database.MigrateAsync();

        // ── Seed SiteContent singleton ───────────────────────────────────────
        // The CHECK constraint + unique index guarantee at most one record at the
        // database level. This initializer ensures the record exists at startup.
        if (!await context.SiteContents.AnyAsync())
        {
            context.SiteContents.Add(new SiteContent
            {
                HeroTitle = "الصحة الطبيعية بين يديك",
                HeroHighlight = "أعشاب السقا",
                CtaEyebrow = "ابدأ رحلتك مع الصحة الطبيعية",
                CtaTitle = "جرب أعشابنا الطبيعية الآن",
                CtaDescription = "منتجات طبيعية 100% مصنوعة بعناية لصحتك ورفاهيتك",
                WhatsAppNumber = "201000626004",
                UpdatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
            logger.LogInformation("SiteContent default record created.");
        }

        // ── Seed GovernorateShipping from governorates.json ─────────────────
        if (!await context.GovernorateShippings.AnyAsync())
        {
            var governorates = new List<GovernorateShipping>
            {
                new() { GovernorateKey = "cairo",         GovernorateName = "القاهرة",        ShippingCost = 20,  IsActive = true },
                new() { GovernorateKey = "giza",          GovernorateName = "الجيزة",         ShippingCost = 25,  IsActive = true },
                new() { GovernorateKey = "alexandria",    GovernorateName = "الإسكندرية",     ShippingCost = 45,  IsActive = true },
                new() { GovernorateKey = "dakahlia",      GovernorateName = "الدقهلية",       ShippingCost = 40,  IsActive = true },
                new() { GovernorateKey = "red-sea",       GovernorateName = "البحر الأحمر",   ShippingCost = 80,  IsActive = true },
                new() { GovernorateKey = "beheira",       GovernorateName = "البحيرة",        ShippingCost = 45,  IsActive = true },
                new() { GovernorateKey = "fayoum",        GovernorateName = "الفيوم",         ShippingCost = 40,  IsActive = true },
                new() { GovernorateKey = "gharbia",       GovernorateName = "الغربية",        ShippingCost = 40,  IsActive = true },
                new() { GovernorateKey = "ismailia",      GovernorateName = "الإسماعيلية",    ShippingCost = 45,  IsActive = true },
                new() { GovernorateKey = "monufia",       GovernorateName = "المنوفية",       ShippingCost = 35,  IsActive = true },
                new() { GovernorateKey = "minya",         GovernorateName = "المنيا",         ShippingCost = 55,  IsActive = true },
                new() { GovernorateKey = "qalyubia",      GovernorateName = "القليوبية",      ShippingCost = 30,  IsActive = true },
                new() { GovernorateKey = "new-valley",    GovernorateName = "الوادي الجديد",  ShippingCost = 90,  IsActive = true },
                new() { GovernorateKey = "suez",          GovernorateName = "السويس",         ShippingCost = 50,  IsActive = true },
                new() { GovernorateKey = "aswan",         GovernorateName = "أسوان",          ShippingCost = 75,  IsActive = true },
                new() { GovernorateKey = "assiut",        GovernorateName = "أسيوط",          ShippingCost = 60,  IsActive = true },
                new() { GovernorateKey = "beni-suef",     GovernorateName = "بني سويف",       ShippingCost = 50,  IsActive = true },
                new() { GovernorateKey = "port-said",     GovernorateName = "بورسعيد",        ShippingCost = 50,  IsActive = true },
                new() { GovernorateKey = "damietta",      GovernorateName = "دمياط",          ShippingCost = 50,  IsActive = true },
                new() { GovernorateKey = "sharqia",       GovernorateName = "الشرقية",        ShippingCost = 40,  IsActive = true },
                new() { GovernorateKey = "south-sinai",   GovernorateName = "جنوب سيناء",     ShippingCost = 90,  IsActive = true },
                new() { GovernorateKey = "kafr-el-sheikh",GovernorateName = "كفر الشيخ",      ShippingCost = 45,  IsActive = true },
                new() { GovernorateKey = "matrouh",       GovernorateName = "مطروح",          ShippingCost = 80,  IsActive = true },
                new() { GovernorateKey = "luxor",         GovernorateName = "الأقصر",         ShippingCost = 70,  IsActive = true },
                new() { GovernorateKey = "qena",          GovernorateName = "قنا",            ShippingCost = 65,  IsActive = true },
                new() { GovernorateKey = "north-sinai",   GovernorateName = "شمال سيناء",     ShippingCost = 100, IsActive = true },
                new() { GovernorateKey = "sohag",         GovernorateName = "سوهاج",          ShippingCost = 65,  IsActive = true },
            };

            context.GovernorateShippings.AddRange(governorates);
            await context.SaveChangesAsync();
            logger.LogInformation("27 GovernorateShipping records seeded.");
        }

        // ── Seed Default Packages ───────────────────────────────────────────
        if (!await context.Packages.AnyAsync())
        {
            var p1 = new Package
            {
                Name = "الباقة التجريبية",
                DurationDays = 15,
                PacketsCount = 1,
                Price = 350,
                OriginalPrice = null,
                PurchaseCount = 142,
                WeightGrams = "500",
                Description = "باقة تجريبية متكاملة لمدة 15 يوماً من الأعشاب الطبيعية المختارة بعناية لدعم صحتك.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            p1.Images.Add(new PackageImage { ImageUrl = "/assets/prod1.png", IsDefault = true });

            var p2 = new Package
            {
                Name = "باقة الشهر",
                DurationDays = 30,
                PacketsCount = 2,
                Price = 620,
                OriginalPrice = 700,
                PurchaseCount = 385,
                WeightGrams = "1000 ~ 950",
                Description = "باقة تكفي لمدة شهر كامل، تمنحك النتائج الأفضل لاستعادة نشاطك وحيويتك الطبيعية.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            p2.Images.Add(new PackageImage { ImageUrl = "/assets/prod2.png", IsDefault = true });

            var p3 = new Package
            {
                Name = "باقة الشهرين",
                DurationDays = 60,
                PacketsCount = 4,
                Price = 1100,
                OriginalPrice = 1400,
                PurchaseCount = 210,
                WeightGrams = "2000",
                Description = "باقة الشهرين الاقتصادية لتجربة مستمرة ونتائج ملحوظة ومستدامة بأعلى جودة.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            p3.Images.Add(new PackageImage { ImageUrl = "/assets/prod3.png", IsDefault = true });

            var p4 = new Package
            {
                Name = "الباقة العائلية",
                DurationDays = 90,
                PacketsCount = 6,
                Price = 1550,
                OriginalPrice = 2100,
                PurchaseCount = 98,
                WeightGrams = "3000",
                Description = "الباقة العائلية الكبرى بأفضل توفير وقيمة لجميع أفراد الأسرة مع استمرارية كاملة.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            p4.Images.Add(new PackageImage { ImageUrl = "/assets/prod1.png", IsDefault = true });

            context.Packages.AddRange(p1, p2, p3, p4);
            await context.SaveChangesAsync();
            logger.LogInformation("Default Packages seeded.");
        }

        // ── Seed Customer Review Images ─────────────────────────────────────
        var reviewsFolder = Path.Combine(
        environment.WebRootPath,
        "assets",
        "Reviews" );

        if (!await context.CustomerReviewImages.AnyAsync())
        {
            if (Directory.Exists(reviewsFolder))
            {
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                var imageFiles = Directory.GetFiles(reviewsFolder)
                    .Where(file => imageExtensions.Contains(
                        Path.GetExtension(file),
                        StringComparer.OrdinalIgnoreCase))
                    .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var reviews = imageFiles.Select(file => new CustomerReviewImage
                {
                    ImageUrl = $"/assets/Reviews/{Path.GetFileName(file)}",
                    IsVisible = true,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                if (reviews.Any())
                {
                    context.CustomerReviewImages.AddRange(reviews);
                    await context.SaveChangesAsync();

                    logger.LogInformation(
                        "{Count} Default Review Images seeded.",
                        reviews.Count);
                }
            }
        }
    }
}
