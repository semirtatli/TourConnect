using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TourConnect.TourService.Application.Interfaces;
using TourConnect.TourService.Infrastructure.Consumers;
using TourConnect.TourService.Infrastructure.Data;
using TourConnect.TourService.Infrastructure.Services;
using MassTransit;

namespace TourConnect.TourService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TourDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("TourConnect.TourService")));

        services.AddScoped<ITourDbContext>(sp => sp.GetRequiredService<TourDbContext>());

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]!));

        services.AddHostedService<DealExpiryService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<ReservationRequestedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
