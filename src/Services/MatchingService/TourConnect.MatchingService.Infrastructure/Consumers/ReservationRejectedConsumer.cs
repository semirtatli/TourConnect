using EventBus.Messages.Events;
using MassTransit;
using TourConnect.MatchingService.Domain.Entities;
using TourConnect.MatchingService.Infrastructure.Data;

namespace TourConnect.MatchingService.Infrastructure.Consumers;

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
