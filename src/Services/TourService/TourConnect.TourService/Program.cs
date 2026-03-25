using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;
using StackExchange.Redis;
using TourConnect.TourService.Application;
using TourConnect.TourService.Application.Deals.Commands;
using TourConnect.TourService.Application.Deals.Queries;
using TourConnect.TourService.Application.DTOs;
using TourConnect.TourService.Application.Operators.Commands;
using TourConnect.TourService.Application.Operators.Queries;
using TourConnect.TourService.Application.Tours.Commands;
using TourConnect.TourService.Application.Tours.Queries;
using TourConnect.TourService.Infrastructure;
using TourConnect.TourService.Infrastructure.Data;
using TourConnect.TourService.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// --- Clean Architecture DI ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- HEALTH CHECKS ---
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TourDbContext>("database")
    .AddRedis(builder.Configuration["Redis:ConnectionString"]!, "redis");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// --- GLOBAL EXCEPTION HANDLER ---
app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
{
    ctx.Response.ContentType = "application/json";

    var exception = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    if (exception is ValidationException validationEx)
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsJsonAsync(new
        {
            error = "Doğrulama hatası.",
            details = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
        });
    }
    else
    {
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsJsonAsync(new { error = "Sunucu hatası." });
    }
}));

// --- MİGRASYON + SEED ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TourDbContext>();
    db.Database.Migrate();
    await TourServiceSeed.SeedAsync(db);
}

var jsonOptions = new JsonSerializerOptions
{
    ReferenceHandler = ReferenceHandler.IgnoreCycles
};

// =====================================================================
// ENDPOINTS — iş mantığı Application katmanında, burada sadece mapping
// =====================================================================

var operators = app.MapGroup("/api/operators");

operators.MapGet("/", async (IMediator mediator) =>
    Results.Ok(await mediator.Send(new GetOperatorsQuery())));

operators.MapPost("/", async (IMediator mediator, CreateOperatorDto dto) =>
{
    var result = await mediator.Send(new CreateOperatorCommand(dto));
    return Results.Created($"/api/operators/{result.Id}", result);
});

var tours = app.MapGroup("/api/tours");

tours.MapGet("/", async (IMediator mediator) =>
    Results.Ok(await mediator.Send(new GetToursQuery())));

tours.MapPost("/", async (IMediator mediator, CreateTourDto dto) =>
{
    var result = await mediator.Send(new CreateTourCommand(dto));
    return result is null ? Results.NotFound("Operatör bulunamadı.") : Results.Created($"/api/tours/{result.Id}", result);
});

var deals = app.MapGroup("/api/deals");

deals.MapGet("/", async (IMediator mediator, IConnectionMultiplexer redis) =>
{
    var cache = redis.GetDatabase();
    var cached = await cache.StringGetAsync("active-deals");
    if (cached.HasValue)
        return Results.Ok(JsonSerializer.Deserialize<List<DealDto>>((string)cached!, jsonOptions));

    var activeDeals = await mediator.Send(new GetActiveDealsQuery());

    await cache.StringSetAsync("active-deals",
        JsonSerializer.Serialize(activeDeals, jsonOptions),
        TimeSpan.FromSeconds(60));

    return Results.Ok(activeDeals);
});

deals.MapPost("/", async (IMediator mediator, IConnectionMultiplexer redis, CreateDealDto dto) =>
{
    var result = await mediator.Send(new CreateDealCommand(dto));
    if (result is null) return Results.NotFound("Tur bulunamadı.");

    await redis.GetDatabase().KeyDeleteAsync("active-deals");

    return Results.Created($"/api/deals/{result.Id}", result);
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
