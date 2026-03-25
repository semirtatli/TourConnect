using Microsoft.EntityFrameworkCore;
using TourConnect.TourService.Domain.Entities;

namespace TourConnect.TourService.Application.Interfaces;

public interface ITourDbContext
{
    DbSet<Operator> Operators { get; }
    DbSet<Tour> Tours { get; }
    DbSet<Deal> Deals { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
