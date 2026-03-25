using EventBus.Messages.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TourConnect.TourService.Data;
using TourConnect.TourService.Entities;

namespace TourConnect.TourService.Services;

// BackgroundService: uygulama ayağa kalktığında otomatik başlar, kapanana kadar çalışır.
// Her 60 saniyede bir süresi dolmuş deal'ları kontrol eder.
// Bu servis Faz 1'de Prototype'ta da vardı — burada aynı sorumluluk ama
// artık event yayınlıyor: Matching Service gelecekte bu event'i dinleyebilir.
public class DealExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DealExpiryService> _logger;

    // IServiceScopeFactory: BackgroundService singleton, DbContext scoped.
    // Doğrudan inject edemeyiz — her kontrol döngüsünde yeni bir scope açarız.
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

        // Süresi dolmuş ama hâlâ Active olan deal'ları bul
        var expiredDeals = await db.Deals
            .Where(d => d.Status == DealStatus.Active && d.ExpiresAt < now)
            .ToListAsync();

        if (expiredDeals.Count == 0) return;

        _logger.LogInformation("{Count} adet deal süresi doldu, Expired yapılıyor.", expiredDeals.Count);

        foreach (var deal in expiredDeals)
        {
            deal.Status = DealStatus.Expired;

            // DealExpiredEvent: Matching Service gelecekte bekleyen rezervasyonları
            // otomatik reddetmek için bu event'i dinleyebilir.
            await publish.Publish(new DealExpiredEvent(
                EventId: Guid.NewGuid(),
                CreatedAt: now,
                DealId: deal.Id));
        }

        await db.SaveChangesAsync();

        // Deal durumları değişti → cache'i temizle
        await redis.GetDatabase().KeyDeleteAsync("active-deals");
    }
}
