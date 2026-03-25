using Microsoft.EntityFrameworkCore;
using TourConnect.MatchingService.Application.Interfaces;
using TourConnect.MatchingService.Domain.Entities;

namespace TourConnect.MatchingService.Infrastructure.Data;

public class MatchingDbContext : DbContext, IMatchingDbContext
{
    public MatchingDbContext(DbContextOptions<MatchingDbContext> options) : base(options) { }

    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
}
