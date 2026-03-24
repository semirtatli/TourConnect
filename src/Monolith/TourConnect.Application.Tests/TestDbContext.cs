using Microsoft.EntityFrameworkCore;
using TourConnect.Application;
using TourConnect.Domain.Entities;

namespace TourConnect.Application.Tests;

// Her test için temiz, izole bir InMemory DB sağlar.
// IAppDbContext'i implemente eder — handler'lar bunu görür, gerçek PostgreSQL'i değil.
public class TestDbContext : DbContext, IAppDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    // Her test metoduna benzersiz DB adı verilerek testler birbirini etkilemez.
    public static TestDbContext Create(string dbName) =>
        new(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options);
}
