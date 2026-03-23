using Microsoft.EntityFrameworkCore;
using TourConnect.Prototype.Data;
using TourConnect.Prototype.Models;

namespace TourConnect.Prototype.Services;

// BackgroundService: .NET'in hosted service base sınıfı.
// "Hosted service" = uygulama yaşam döngüsüne bağlı servis.
// ExecuteAsync override edilerek arka plan işi tanımlanır.
public class DealExpiryService : BackgroundService
{
    // IServiceScopeFactory: background service'lerde DbContext kullanmak için gerekli.
    // Neden doğrudan AppDbContext inject edemiyoruz?
    // AppDbContext "scoped" servis — her HTTP isteği için ayrı instance oluşturulur.
    // BackgroundService ise "singleton" — uygulama boyunca tek instance.
    // Singleton içine scoped servis inject edilemez (lifetime uyuşmazlığı).
    // Çözüm: IServiceScopeFactory ile her döngüde yeni bir scope oluşturuyoruz.
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DealExpiryService> _logger;

    public DealExpiryService(IServiceScopeFactory scopeFactory, ILogger<DealExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // ExecuteAsync: uygulama başlayınca çalışır, CancellationToken iptal edilince durur.
    // CancellationToken: uygulama kapanırken .NET bunu "iptal et" diye işaretler,
    // bu sayede while döngüsü temiz bir şekilde sonlanır.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Deal expiry service başlatıldı.");

        // stoppingToken.IsCancellationRequested → uygulama kapanıyor mu?
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpiredDeals();

            // 60 saniye bekle, sonra tekrar kontrol et.
            // Task.Delay yerine stoppingToken geçiyoruz:
            // uygulama kapanırken 60 sn beklemek yerine hemen durabilsin.
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    private async Task CheckExpiredDeals()
    {
        // Her döngüde yeni bir scope oluştur → yeni bir DbContext instance'ı al.
        // using → scope işi bitince otomatik dispose edilir (bağlantı havuza döner).
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var expiredDeals = await db.Deals
            .Where(d => d.ExpiresAt < DateTime.UtcNow && d.Status == DealStatus.Active)
            .ToListAsync();

        if (expiredDeals.Count == 0)
            return;

        foreach (var deal in expiredDeals)
            deal.Status = DealStatus.Expired;

        await db.SaveChangesAsync();

        _logger.LogInformation("{Count} deal expired olarak işaretlendi.", expiredDeals.Count);
    }
}
