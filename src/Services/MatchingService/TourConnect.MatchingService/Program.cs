using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;
using TourConnect.MatchingService.Consumers;
using TourConnect.MatchingService.Data;
using TourConnect.MatchingService.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// IgnoreCycles: Reservation→Partner→Reservations→Partner şeklindeki döngüyü kırar.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// --- VERİTABANI ---
builder.Services.AddDbContext<MatchingDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- HEALTH CHECKS ---
// DB: EF Core üzerinden SELECT 1 çalıştırır.
// masstransit-bus: MassTransit RabbitMQ bağlantısını otomatik olarak ekler.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MatchingDbContext>("database");

// --- MASSTRANSIT + RABBITMQ ---
builder.Services.AddMassTransit(x =>
{
    // Matching Service iki event dinler: Confirmed ve Rejected
    x.AddConsumer<ReservationConfirmedConsumer>();
    x.AddConsumer<ReservationRejectedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        cfg.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// --- GLOBAL EXCEPTION HANDLER ---
app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
{
    ctx.Response.StatusCode = 500;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(new { error = "Sunucu hatası." });
}));

// --- MİGRASYON + SEED ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MatchingDbContext>();
    db.Database.Migrate();

    // Sabit GUID'ler Monolith seed'iyle birebir aynı — sistemler arası tutarlılık.
    var partner1Id = Guid.Parse("d1000000-0000-0000-0000-000000000001");
    if (!await db.Partners.AnyAsync(p => p.Id == partner1Id))
    {
        var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        db.Partners.AddRange(
            new Partner { Id = partner1Id,                                          Name = "Grand Hotel Bodrum",   ContactEmail = "concierge@grandhotelbodrum.com", Location = "Bodrum",  CreatedAt = seededAt },
            new Partner { Id = Guid.Parse("d2000000-0000-0000-0000-000000000002"), Name = "Antalya Palace Hotel", ContactEmail = "tours@antalyapalace.com",        Location = "Antalya", CreatedAt = seededAt }
        );
        await db.SaveChangesAsync();
    }
}

// =====================================================================
// ENDPOINTS
// =====================================================================

var partners = app.MapGroup("/api/partners");

partners.MapGet("/", async (MatchingDbContext db) =>
    Results.Ok(await db.Partners.ToListAsync()));

partners.MapPost("/", async (MatchingDbContext db, Partner partner) =>
{
    partner.Id = Guid.NewGuid();
    db.Partners.Add(partner);
    await db.SaveChangesAsync();
    return Results.Created($"/api/partners/{partner.Id}", partner);
});

var reservations = app.MapGroup("/api/reservations");

reservations.MapGet("/", async (MatchingDbContext db) =>
    Results.Ok(await db.Reservations.Include(r => r.Partner).ToListAsync()));

reservations.MapGet("/{id:guid}", async (MatchingDbContext db, Guid id) =>
{
    var reservation = await db.Reservations.FindAsync(id);
    return reservation is null ? Results.NotFound() : Results.Ok(reservation);
});

reservations.MapPost("/", async (MatchingDbContext db, IPublishEndpoint publish, Reservation reservation) =>
{
    var partnerExists = await db.Partners.AnyAsync(p => p.Id == reservation.PartnerId);
    if (!partnerExists) return Results.NotFound("Partner bulunamadı.");

    // Rezervasyonu Pending olarak kaydet
    reservation.Id = Guid.NewGuid();
    reservation.Status = ReservationStatus.Pending;
    db.Reservations.Add(reservation);
    await db.SaveChangesAsync();

    // ReservationRequestedEvent yayınla → Tour Service dinliyor
    // HTTP çağrısı yok — sadece event. Tour Service çökmüş olsa bile bu satır başarılı olur.
    await publish.Publish(new ReservationRequestedEvent(
        EventId: Guid.NewGuid(),
        CreatedAt: DateTime.UtcNow,
        ReservationId: reservation.Id,
        DealId: reservation.DealId,
        PartnerId: reservation.PartnerId,
        GuestName: reservation.GuestName,
        GuestCount: reservation.GuestCount));

    // 202 Accepted: iş henüz bitmedi, ama kabul edildi
    return Results.Accepted($"/api/reservations/{reservation.Id}", reservation);
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
