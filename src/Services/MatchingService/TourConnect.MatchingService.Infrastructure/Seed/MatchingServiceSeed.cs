using Microsoft.EntityFrameworkCore;
using TourConnect.MatchingService.Domain.Entities;
using TourConnect.MatchingService.Infrastructure.Data;

namespace TourConnect.MatchingService.Infrastructure.Seed;

public static class MatchingServiceSeed
{
    public static async Task SeedAsync(MatchingDbContext db)
    {
        var partner1Id = Guid.Parse("d1000000-0000-0000-0000-000000000001");
        if (await db.Partners.AnyAsync(p => p.Id == partner1Id)) return;

        var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        db.Partners.AddRange(
            new Partner { Id = partner1Id, Name = "Grand Hotel Bodrum", ContactEmail = "concierge@grandhotelbodrum.com", Location = "Bodrum", CreatedAt = seededAt },
            new Partner { Id = Guid.Parse("d2000000-0000-0000-0000-000000000002"), Name = "Antalya Palace Hotel", ContactEmail = "tours@antalyapalace.com", Location = "Antalya", CreatedAt = seededAt }
        );
        await db.SaveChangesAsync();
    }
}
