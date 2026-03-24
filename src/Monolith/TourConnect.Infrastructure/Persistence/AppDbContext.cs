using Microsoft.EntityFrameworkCore;
using TourConnect.Application;
using TourConnect.Domain.Entities;

namespace TourConnect.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
}
