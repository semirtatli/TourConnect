using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using TourConnect.TourService.Consumers;
using TourConnect.TourService.Data;
using TourConnect.TourService.Entities;
using TourConnect.TourService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- JSON ---
// IgnoreCycles: Deal→Tour→Deals→Deal şeklindeki döngüsel referansları kırar.
// Olmadan serializer sonsuz döngüye girer ve 500 fırlatır.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// --- VERİTABANI ---
// Tour Service'in kendine ait PostgreSQL DB'si.
// Matching Service bu bağlantıya sahip değil — tam DB izolasyonu.
builder.Services.AddDbContext<TourDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- REDIS ---
// Aktif deal'ları cache'lemek için. Sık okunan veriyi DB'ye gitmeden sunar.
// ConnectionMultiplexer: Redis bağlantısını paylaşan singleton nesne.
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));

// --- HEALTH CHECKS ---
// DB: EF Core üzerinden SELECT 1 çalıştırır.
// Redis: PING komutu gönderir.
// masstransit-bus: MassTransit RabbitMQ bağlantısını otomatik olarak ekler.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TourDbContext>("database")
    .AddRedis(builder.Configuration["Redis:ConnectionString"]!, "redis");

// --- DEAL EXPIRY SERVICE ---
// BackgroundService: her 60 saniyede bir süresi dolmuş deal'ları Expired yapar.
// Singleton lifecycle'ı olan BackgroundService, IServiceScopeFactory aracılığıyla
// scoped DbContext'e erişir.
builder.Services.AddHostedService<DealExpiryService>();

// --- MASSTRANSIT + RABBITMQ ---
// MassTransit: message bus abstraction. RabbitMQ'nun karmaşıklığını gizler.
// Biz sadece "Publish" ve "Consumer" yazarız, kuyruk yönetimini MassTransit halleder.
builder.Services.AddMassTransit(x =>
{
    // Consumer kaydı: MassTransit bu sınıfın hangi event'i dinleyeceğini otomatik anlar
    x.AddConsumer<ReservationRequestedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        // Consumer'ı endpoint'e bağla: "reservation-requested" kuyruğunu dinle
        cfg.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// --- GLOBAL EXCEPTION HANDLER ---
// Ham stack trace yerine temiz bir JSON hata mesajı döner.
app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
{
    ctx.Response.StatusCode = 500;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(new { error = "Sunucu hatası." });
}));

// --- MİGRASYON + SEED ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TourDbContext>();
    db.Database.Migrate();

    // Sabit GUID'ler Monolith seed'iyle birebir aynı — sistemler arası tutarlılık.
    var op1Id = Guid.Parse("a1000000-0000-0000-0000-000000000001");
    if (!await db.Operators.AnyAsync(o => o.Id == op1Id))
    {
        var op2Id      = Guid.Parse("a2000000-0000-0000-0000-000000000002");
        var tour1Id    = Guid.Parse("b1000000-0000-0000-0000-000000000001");
        var tour2Id    = Guid.Parse("b2000000-0000-0000-0000-000000000002");
        var tour3Id    = Guid.Parse("b3000000-0000-0000-0000-000000000003");
        var seededAt   = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var neverExpires = new DateTime(2099, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        db.Operators.AddRange(
            new Operator { Id = op1Id, Name = "Aegean Blue Tours",       Phone = "+90 252 316 4500", Location = "Bodrum",  CreatedAt = seededAt },
            new Operator { Id = op2Id, Name = "Mediterranean Adventures", Phone = "+90 242 247 8800", Location = "Antalya", CreatedAt = seededAt }
        );
        db.Tours.AddRange(
            new Tour { Id = tour1Id, OperatorId = op1Id, Title = "Bodrum Tekne Turu",    Description = "Günnük körfezinin masmavi sularında günlük tekne turu.", Category = "BoatTour", DurationInHours = 8, BasePrice = 500, CreatedAt = seededAt },
            new Tour { Id = tour2Id, OperatorId = op1Id, Title = "Bodrum Dalış Macerası", Description = "Ege'nin kristal sularında profesyonel rehber eşliğinde dalış.", Category = "Diving", DurationInHours = 5, BasePrice = 800, CreatedAt = seededAt },
            new Tour { Id = tour3Id, OperatorId = op2Id, Title = "Belek Jeep Safari",     Description = "Toroslar'ın eteklerinde macera dolu jeep safari turu.", Category = "Safari",   DurationInHours = 6, BasePrice = 350, CreatedAt = seededAt }
        );
        db.Deals.AddRange(
            new Deal { Id = Guid.Parse("c1000000-0000-0000-0000-000000000001"), TourId = tour1Id, OperatorId = op1Id, AvailableSlots = 8,  OriginalPrice = 500, DiscountedPrice = 350, ExpiresAt = neverExpires, Status = DealStatus.Active, CreatedAt = seededAt },
            new Deal { Id = Guid.Parse("c2000000-0000-0000-0000-000000000002"), TourId = tour2Id, OperatorId = op1Id, AvailableSlots = 4,  OriginalPrice = 800, DiscountedPrice = 550, ExpiresAt = neverExpires, Status = DealStatus.Active, CreatedAt = seededAt },
            new Deal { Id = Guid.Parse("c3000000-0000-0000-0000-000000000003"), TourId = tour3Id, OperatorId = op2Id, AvailableSlots = 12, OriginalPrice = 350, DiscountedPrice = 240, ExpiresAt = neverExpires, Status = DealStatus.Active, CreatedAt = seededAt }
        );
        await db.SaveChangesAsync();
    }
}

// Redis'e yazarken de döngüsel referans sorununu önlemek için ayrı options.
var jsonOptions = new JsonSerializerOptions
{
    ReferenceHandler = ReferenceHandler.IgnoreCycles
};

// =====================================================================
// ENDPOINTS
// =====================================================================

var operators = app.MapGroup("/api/operators");

operators.MapGet("/", async (TourDbContext db) =>
    Results.Ok(await db.Operators.ToListAsync()));

operators.MapPost("/", async (TourDbContext db, Operator op) =>
{
    op.Id = Guid.NewGuid();
    db.Operators.Add(op);
    await db.SaveChangesAsync();
    return Results.Created($"/api/operators/{op.Id}", op);
});

var tours = app.MapGroup("/api/tours");

tours.MapGet("/", async (TourDbContext db) =>
    Results.Ok(await db.Tours.Include(t => t.Operator).ToListAsync()));

tours.MapPost("/", async (TourDbContext db, Tour tour) =>
{
    // Operator var mı kontrol et — FK violation yerine anlamlı 404 dön
    var operatorExists = await db.Operators.AnyAsync(o => o.Id == tour.OperatorId);
    if (!operatorExists) return Results.NotFound("Operatör bulunamadı.");

    tour.Id = Guid.NewGuid();
    db.Tours.Add(tour);
    await db.SaveChangesAsync();
    return Results.Created($"/api/tours/{tour.Id}", tour);
});

var deals = app.MapGroup("/api/deals");

deals.MapGet("/", async (TourDbContext db, IConnectionMultiplexer redis) =>
{
    // Önce Redis'e bak
    var cache = redis.GetDatabase();
    var cached = await cache.StringGetAsync("active-deals");
    if (cached.HasValue)
        return Results.Ok(JsonSerializer.Deserialize<List<Deal>>((string)cached!, jsonOptions));

    // Cache'de yoksa DB'den çek ve cache'e yaz (60 sn TTL)
    var activeDeals = await db.Deals
        .Where(d => d.Status == DealStatus.Active)
        .Include(d => d.Tour)
        .ToListAsync();

    // jsonOptions ile serialize: döngüsel referansı kırar
    await cache.StringSetAsync("active-deals",
        JsonSerializer.Serialize(activeDeals, jsonOptions),
        TimeSpan.FromSeconds(60));

    return Results.Ok(activeDeals);
});

deals.MapPost("/", async (TourDbContext db, IConnectionMultiplexer redis, Deal deal) =>
{
    var tour = await db.Tours.FindAsync(deal.TourId);
    if (tour is null) return Results.NotFound("Tur bulunamadı.");

    deal.Id = Guid.NewGuid();
    deal.OperatorId = tour.OperatorId;
    deal.Status = DealStatus.Active;
    db.Deals.Add(deal);
    await db.SaveChangesAsync();

    // Yeni deal eklendi → cache'i geçersiz kıl
    var cache = redis.GetDatabase();
    await cache.KeyDeleteAsync("active-deals");

    return Results.Created($"/api/deals/{deal.Id}", deal);
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                e => e.Key,
                e => e.Value.Status.ToString())
        });
    }
});

app.Run();
