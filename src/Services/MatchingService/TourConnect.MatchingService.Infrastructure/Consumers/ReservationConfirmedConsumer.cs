using EventBus.Messages.Events;
using MassTransit;
using TourConnect.MatchingService.Domain.Entities;
using TourConnect.MatchingService.Infrastructure.Data;

namespace TourConnect.MatchingService.Infrastructure.Consumers;

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
