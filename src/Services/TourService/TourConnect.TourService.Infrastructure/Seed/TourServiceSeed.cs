using Microsoft.EntityFrameworkCore;
using TourConnect.TourService.Domain.Entities;
using TourConnect.TourService.Infrastructure.Data;

namespace TourConnect.TourService.Infrastructure.Seed;

public static class TourServiceSeed
{
    public static async Task SeedAsync(TourDbContext db)
    {
        var op1Id = Guid.Parse("a1000000-0000-0000-0000-000000000001");
        if (await db.Operators.AnyAsync(o => o.Id == op1Id)) return;

        var op2Id = Guid.Parse("a2000000-0000-0000-0000-000000000002");
        var tour1Id = Guid.Parse("b1000000-0000-0000-0000-000000000001");
        var tour2Id = Guid.Parse("b2000000-0000-0000-0000-000000000002");
        var tour3Id = Guid.Parse("b3000000-0000-0000-0000-000000000003");
        var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var neverExpires = new DateTime(2099, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        db.Operators.AddRange(
            new Operator { Id = op1Id, Name = "Aegean Blue Tours", Phone = "+90 252 316 4500", Location = "Bodrum", CreatedAt = seededAt },
            new Operator { Id = op2Id, Name = "Mediterranean Adventures", Phone = "+90 242 247 8800", Location = "Antalya", CreatedAt = seededAt }
        );
        db.Tours.AddRange(
            new Tour { Id = tour1Id, OperatorId = op1Id, Title = "Bodrum Tekne Turu", Description = "Günnük körfezinin masmavi sularında günlük tekne turu.", Category = "BoatTour", DurationInHours = 8, BasePrice = 500, CreatedAt = seededAt },
            new Tour { Id = tour2Id, OperatorId = op1Id, Title = "Bodrum Dalış Macerası", Description = "Ege'nin kristal sularında profesyonel rehber eşliğinde dalış.", Category = "Diving", DurationInHours = 5, BasePrice = 800, CreatedAt = seededAt },
            new Tour { Id = tour3Id, OperatorId = op2Id, Title = "Belek Jeep Safari", Description = "Toroslar'ın eteklerinde macera dolu jeep safari turu.", Category = "Safari", DurationInHours = 6, BasePrice = 350, CreatedAt = seededAt }
        );
        db.Deals.AddRange(
            new Deal { Id = Guid.Parse("c1000000-0000-0000-0000-000000000001"), TourId = tour1Id, OperatorId = op1Id, AvailableSlots = 8, OriginalPrice = 500, DiscountedPrice = 350, ExpiresAt = neverExpires, Status = DealStatus.Active, CreatedAt = seededAt },
            new Deal { Id = Guid.Parse("c2000000-0000-0000-0000-000000000002"), TourId = tour2Id, OperatorId = op1Id, AvailableSlots = 4, OriginalPrice = 800, DiscountedPrice = 550, ExpiresAt = neverExpires, Status = DealStatus.Active, CreatedAt = seededAt },
            new Deal { Id = Guid.Parse("c3000000-0000-0000-0000-000000000003"), TourId = tour3Id, OperatorId = op2Id, AvailableSlots = 12, OriginalPrice = 350, DiscountedPrice = 240, ExpiresAt = neverExpires, Status = DealStatus.Active, CreatedAt = seededAt }
        );
        await db.SaveChangesAsync();
    }
}
