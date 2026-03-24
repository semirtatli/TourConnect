using Microsoft.EntityFrameworkCore;
using TourConnect.Domain.Entities;

namespace TourConnect.Application;

// Application katmanı DB'ye doğrudan bağımlı değil — bu interface üzerinden konuşur.
// Infrastructure.AppDbContext bu interface'i implemente eder.
// Böylece ileride test yazarken gerçek DB yerine InMemory DB kullanabiliriz.
public interface IAppDbContext
{
    DbSet<Operator> Operators { get; }
    DbSet<Tour> Tours { get; }
    DbSet<Deal> Deals { get; }
    DbSet<Partner> Partners { get; }
    DbSet<Reservation> Reservations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
