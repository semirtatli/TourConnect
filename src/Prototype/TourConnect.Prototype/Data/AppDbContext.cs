using TourConnect.Prototype.Models;

namespace TourConnect.Prototype.Data;

// AppDbContext: EF Core'un veritabanıyla konuştuğu köprü sınıf.
// Faz 0'da Program.cs'deydi. Artık kendi dosyasında — Data katmanının başlangıcı.
// Faz 2'de Infrastructure katmanına taşınacak.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
}
