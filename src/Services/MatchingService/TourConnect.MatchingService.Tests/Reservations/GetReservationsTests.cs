using FluentAssertions;
using TourConnect.MatchingService.Application.Reservations.Queries;
using TourConnect.MatchingService.Domain.Entities;

namespace TourConnect.MatchingService.Tests.Reservations;

public class GetReservationsTests
{
    [Fact]
    public async Task Handle_ReturnsAllReservations()
    {
        using var db = TestDbContextFactory.Create();
        var partner = new Partner { Name = "Hotel", ContactEmail = "a@a.com", Location = "A" };
        db.Partners.Add(partner);
        db.Reservations.AddRange(
            new Reservation { DealId = Guid.NewGuid(), PartnerId = partner.Id, GuestName = "Ali", GuestCount = 2 },
            new Reservation { DealId = Guid.NewGuid(), PartnerId = partner.Id, GuestName = "Veli", GuestCount = 1 });
        await db.SaveChangesAsync();

        var handler = new GetReservationsHandler(db);
        var result = await handler.Handle(new GetReservationsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ReturnsCorrectReservation()
    {
        using var db = TestDbContextFactory.Create();
        var partner = new Partner { Name = "Hotel", ContactEmail = "a@a.com", Location = "A" };
        db.Partners.Add(partner);
        var reservation = new Reservation { DealId = Guid.NewGuid(), PartnerId = partner.Id, GuestName = "Ali", GuestCount = 2 };
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync();

        var handler = new GetReservationByIdHandler(db);
        var result = await handler.Handle(new GetReservationByIdQuery(reservation.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.GuestName.Should().Be("Ali");
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetReservationByIdHandler(db);
        var result = await handler.Handle(new GetReservationByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
