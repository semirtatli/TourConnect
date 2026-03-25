using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;
using TourConnect.MatchingService.Application;
using TourConnect.MatchingService.Application.DTOs;
using TourConnect.MatchingService.Application.Partners.Commands;
using TourConnect.MatchingService.Application.Partners.Queries;
using TourConnect.MatchingService.Application.Reservations.Commands;
using TourConnect.MatchingService.Application.Reservations.Queries;
using TourConnect.MatchingService.Infrastructure;
using TourConnect.MatchingService.Infrastructure.Data;
using TourConnect.MatchingService.Infrastructure.Seed;

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
    .AddDbContextCheck<MatchingDbContext>("database");

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
    var db = scope.ServiceProvider.GetRequiredService<MatchingDbContext>();
    db.Database.Migrate();
    await MatchingServiceSeed.SeedAsync(db);
}

// =====================================================================
// ENDPOINTS — iş mantığı Application katmanında, burada sadece mapping
// =====================================================================

var partners = app.MapGroup("/api/partners");

partners.MapGet("/", async (IMediator mediator) =>
    Results.Ok(await mediator.Send(new GetPartnersQuery())));

partners.MapPost("/", async (IMediator mediator, CreatePartnerDto dto) =>
{
    var result = await mediator.Send(new CreatePartnerCommand(dto));
    return Results.Created($"/api/partners/{result.Id}", result);
});

var reservations = app.MapGroup("/api/reservations");

reservations.MapGet("/", async (IMediator mediator) =>
    Results.Ok(await mediator.Send(new GetReservationsQuery())));

reservations.MapGet("/{id:guid}", async (IMediator mediator, Guid id) =>
{
    var result = await mediator.Send(new GetReservationByIdQuery(id));
    return result is null ? Results.NotFound() : Results.Ok(result);
});

reservations.MapPost("/", async (IMediator mediator, CreateReservationDto dto) =>
{
    var result = await mediator.Send(new CreateReservationCommand(dto));
    return result is null
        ? Results.NotFound("Partner bulunamadı.")
        : Results.Accepted($"/api/reservations/{result.Id}", result);
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
