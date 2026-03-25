using EventBus.Messages.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TourConnect.MatchingService.Data;
using TourConnect.MatchingService.Entities;

namespace TourConnect.MatchingService.Consumers;

// Tour Service slot yetersizse bu event'i yayınlar.
// Matching Service burada Reservation.Status = Rejected yapar.
public class ReservationRejectedConsumer : IConsumer<ReservationRejectedEvent>
{
    private readonly MatchingDbContext _db;

    public ReservationRejectedConsumer(MatchingDbContext db) => _db = db;

    public async Task Consume(ConsumeContext<ReservationRejectedEvent> context)
    {
        var reservation = await _db.Reservations.FindAsync(context.Message.ReservationId);
        if (reservation is null) return;

        reservation.Status = ReservationStatus.Rejected;
        reservation.RejectionReason = context.Message.Reason;
        await _db.SaveChangesAsync();
    }
}
