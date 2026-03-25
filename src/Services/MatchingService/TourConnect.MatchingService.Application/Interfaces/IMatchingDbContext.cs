using Microsoft.EntityFrameworkCore;
using TourConnect.MatchingService.Domain.Entities;

namespace TourConnect.MatchingService.Application.Interfaces;

public interface IMatchingDbContext
{
    DbSet<Partner> Partners { get; }
    DbSet<Reservation> Reservations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
