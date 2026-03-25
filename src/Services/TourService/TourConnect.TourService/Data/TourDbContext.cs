using Microsoft.EntityFrameworkCore;
using TourConnect.TourService.Entities;

namespace TourConnect.TourService.Data;

// Tour Service'in kendi DB'si — Matching Service bu DB'ye asla erişemez.
// DB izolasyonu: servisler arası veri paylaşımı sadece event'ler üzerinden olur.
public class TourDbContext : DbContext
{
    public TourDbContext(DbContextOptions<TourDbContext> options) : base(options) { }

    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<Deal> Deals => Set<Deal>();
}
