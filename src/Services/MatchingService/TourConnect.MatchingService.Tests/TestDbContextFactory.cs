using Microsoft.EntityFrameworkCore;
using TourConnect.MatchingService.Infrastructure.Data;

namespace TourConnect.MatchingService.Tests;

public static class TestDbContextFactory
{
    public static MatchingDbContext Create()
    {
        var options = new DbContextOptionsBuilder<MatchingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MatchingDbContext(options);
    }
}
