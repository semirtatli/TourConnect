using EventBus.Messages.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TourConnect.TourService.Domain.Entities;
using TourConnect.TourService.Infrastructure.Data;

namespace TourConnect.TourService.Infrastructure.Services;

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
        _logger.LogInformation("DealExpiryService başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpiredDealsAsync();
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    private async Task CheckExpiredDealsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TourDbContext>();
        var publish = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();

        var now = DateTime.UtcNow;

        var expiredDeals = await db.Deals
            .Where(d => d.Status == DealStatus.Active && d.ExpiresAt < now)
            .ToListAsync();

        if (expiredDeals.Count == 0) return;

        _logger.LogInformation("{Count} adet deal süresi doldu, Expired yapılıyor.", expiredDeals.Count);

        foreach (var deal in expiredDeals)
        {
            deal.Status = DealStatus.Expired;

            await publish.Publish(new DealExpiredEvent(
                EventId: Guid.NewGuid(),
                CreatedAt: now,
                DealId: deal.Id));
        }

        await db.SaveChangesAsync();

        await redis.GetDatabase().KeyDeleteAsync("active-deals");
    }
}
