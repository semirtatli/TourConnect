using EventBus.Messages.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TourConnect.MatchingService.Data;
using TourConnect.MatchingService.Entities;

namespace TourConnect.MatchingService.Consumers;

// Tour Service slot düştükten sonra bu event'i yayınlar.
// Matching Service burada Reservation.Status = Confirmed yapar.
public class ReservationConfirmedConsumer : IConsumer<ReservationConfirmedEvent>
{
    private readonly MatchingDbContext _db;

    public ReservationConfirmedConsumer(MatchingDbContext db) => _db = db;

    public async Task Consume(ConsumeContext<ReservationConfirmedEvent> context)
    {
        var reservation = await _db.Reservations.FindAsync(context.Message.ReservationId);
        if (reservation is null) return;

        reservation.Status = ReservationStatus.Confirmed;
        await _db.SaveChangesAsync();
    }
}
