using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TourConnect.MatchingService.Application.Interfaces;
using TourConnect.MatchingService.Infrastructure.Consumers;
using TourConnect.MatchingService.Infrastructure.Data;

namespace TourConnect.MatchingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MatchingDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("TourConnect.MatchingService")));

        services.AddScoped<IMatchingDbContext>(sp => sp.GetRequiredService<MatchingDbContext>());

        services.AddMassTransit(x =>
        {
            x.AddConsumer<ReservationConfirmedConsumer>();
            x.AddConsumer<ReservationRejectedConsumer>();

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
