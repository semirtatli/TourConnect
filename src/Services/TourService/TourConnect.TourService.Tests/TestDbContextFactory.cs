using Microsoft.EntityFrameworkCore;
using TourConnect.TourService.Infrastructure.Data;

namespace TourConnect.TourService.Tests;

public static class TestDbContextFactory
{
    public static TourDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TourDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TourDbContext(options);
    }
}
