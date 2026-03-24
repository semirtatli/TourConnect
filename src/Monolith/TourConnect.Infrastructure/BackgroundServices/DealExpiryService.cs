using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TourConnect.Domain.Entities;
using TourConnect.Infrastructure.Persistence;

namespace TourConnect.Infrastructure.BackgroundServices;

public class DealExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DealExpiryService> _logger;

    public DealExpiryService(IServiceScopeFactory scopeFactory, ILogger<DealExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpiredDeals();
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    private async Task CheckExpiredDeals()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var expired = await db.Deals
            .Where(d => d.Status == DealStatus.Active && d.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        if (expired.Count == 0) return;

        foreach (var deal in expired)
            deal.Status = DealStatus.Expired;

        await db.SaveChangesAsync();
        _logger.LogInformation("{Count} deal expired.", expired.Count);
    }
}
