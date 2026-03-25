using Microsoft.EntityFrameworkCore;
using TourConnect.Domain.Entities;

namespace TourConnect.Infrastructure.Persistence;

// Sabit GUID'ler: her restart'ta aynı seed data yazılmaz (idempotent).
// Aynı GUID'ler TourService ve MatchingService seed'lerinde de kullanılır.
public static class SeedData
{
    // Operatörler
    public static readonly Guid Op1Id = Guid.Parse("a1000000-0000-0000-0000-000000000001");
    public static readonly Guid Op2Id = Guid.Parse("a2000000-0000-0000-0000-000000000002");

    // Turlar
    public static readonly Guid Tour1Id = Guid.Parse("b1000000-0000-0000-0000-000000000001");
    public static readonly Guid Tour2Id = Guid.Parse("b2000000-0000-0000-0000-000000000002");
    public static readonly Guid Tour3Id = Guid.Parse("b3000000-0000-0000-0000-000000000003");

    // Deal'lar
    public static readonly Guid Deal1Id = Guid.Parse("c1000000-0000-0000-0000-000000000001");
    public static readonly Guid Deal2Id = Guid.Parse("c2000000-0000-0000-0000-000000000002");
    public static readonly Guid Deal3Id = Guid.Parse("c3000000-0000-0000-0000-000000000003");

    // Partner'lar
    public static readonly Guid Partner1Id = Guid.Parse("d1000000-0000-0000-0000-000000000001");
    public static readonly Guid Partner2Id = Guid.Parse("d2000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(AppDbContext db)
    {
        // Seed GUID'i zaten varsa çık — önceki test verileri varsa bile seed'i çalıştırır.
        if (await db.Operators.AnyAsync(o => o.Id == Op1Id))
            return;

        // Sabit zaman damgası: her restart'ta aynı CreatedAt değeri korunur.
        var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var operators = new List<Operator>
        {
            new() {
                Id = Op1Id,
                Name = "Aegean Blue Tours",
                Phone = "+90 252 316 4500",
                Location = "Bodrum",
                IsActive = true,
                CreatedAt = seededAt
            },
            new() {
                Id = Op2Id,
                Name = "Mediterranean Adventures",
                Phone = "+90 242 247 8800",
                Location = "Antalya",
                IsActive = true,
                CreatedAt = seededAt
            }
        };

        var tours = new List<Tour>
        {
            new() {
                Id = Tour1Id,
                OperatorId = Op1Id,
                Title = "Bodrum Tekne Turu",
                Description = "Günnük körfezinin masmavi sularında, Karaada ve Kara Ada koylarını kapsayan günlük tekne turu.",
                Category = TourCategory.BoatTour,
                DurationInHours = 8,
                BasePrice = 500,
                CreatedAt = seededAt
            },
            new() {
                Id = Tour2Id,
                OperatorId = Op1Id,
                Title = "Bodrum Dalış Macerası",
                Description = "Ege'nin kristal sularında profesyonel rehber eşliğinde dalış deneyimi. Başlangıç seviyesine uygun.",
                Category = TourCategory.Diving,
                DurationInHours = 5,
                BasePrice = 800,
                CreatedAt = seededAt
            },
            new() {
                Id = Tour3Id,
                OperatorId = Op2Id,
                Title = "Belek Jeep Safari",
                Description = "Toroslar'ın eteklerinde nefes kesen manzaralar eşliğinde macera dolu jeep safari turu.",
                Category = TourCategory.Safari,
                DurationInHours = 6,
                BasePrice = 350,
                CreatedAt = seededAt
            }
        };

        // ExpiresAt: seed deal'leri hiçbir zaman expire olmamalı.
        // DealExpiryService gerçek senaryoları yönetir, seed verisini değil.
        var neverExpires = new DateTime(2099, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        var deals = new List<Deal>
        {
            new() {
                Id = Deal1Id,
                TourId = Tour1Id,
                OperatorId = Op1Id,
                AvailableSlots = 8,
                OriginalPrice = 500,
                DiscountedPrice = 350,
                ExpiresAt = neverExpires,
                Status = DealStatus.Active,
                CreatedAt = seededAt
            },
            new() {
                Id = Deal2Id,
                TourId = Tour2Id,
                OperatorId = Op1Id,
                AvailableSlots = 4,
                OriginalPrice = 800,
                DiscountedPrice = 550,
                ExpiresAt = neverExpires,
                Status = DealStatus.Active,
                CreatedAt = seededAt
            },
            new() {
                Id = Deal3Id,
                TourId = Tour3Id,
                OperatorId = Op2Id,
                AvailableSlots = 12,
                OriginalPrice = 350,
                DiscountedPrice = 240,
                ExpiresAt = neverExpires,
                Status = DealStatus.Active,
                CreatedAt = seededAt
            }
        };

        var partners = new List<Partner>
        {
            new() {
                Id = Partner1Id,
                Name = "Grand Hotel Bodrum",
                ContactEmail = "concierge@grandhotelbodrum.com",
                Location = "Bodrum",
                IsActive = true,
                CreatedAt = seededAt
            },
            new() {
                Id = Partner2Id,
                Name = "Antalya Palace Hotel",
                ContactEmail = "tours@antalyapalace.com",
                Location = "Antalya",
                IsActive = true,
                CreatedAt = seededAt
            }
        };

        db.Operators.AddRange(operators);
        db.Tours.AddRange(tours);
        db.Deals.AddRange(deals);
        db.Partners.AddRange(partners);
        await db.SaveChangesAsync();
    }
}
