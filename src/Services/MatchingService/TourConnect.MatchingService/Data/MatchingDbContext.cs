using Microsoft.EntityFrameworkCore;
using TourConnect.MatchingService.Entities;

namespace TourConnect.MatchingService.Data;

public class MatchingDbContext : DbContext
{
    public MatchingDbContext(DbContextOptions<MatchingDbContext> options) : base(options) { }

    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
}
