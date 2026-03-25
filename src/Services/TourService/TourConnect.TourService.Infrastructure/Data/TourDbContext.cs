using Microsoft.EntityFrameworkCore;
using TourConnect.TourService.Application.Interfaces;
using TourConnect.TourService.Domain.Entities;

namespace TourConnect.TourService.Infrastructure.Data;

public class TourDbContext : DbContext, ITourDbContext
{
    public TourDbContext(DbContextOptions<TourDbContext> options) : base(options) { }

    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<Deal> Deals => Set<Deal>();
}
