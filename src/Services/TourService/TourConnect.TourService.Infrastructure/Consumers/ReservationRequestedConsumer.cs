using EventBus.Messages.Events;
using MassTransit;
using StackExchange.Redis;
using TourConnect.TourService.Domain.Entities;
using TourConnect.TourService.Infrastructure.Data;

namespace TourConnect.TourService.Infrastructure.Consumers;

public class ReservationRequestedConsumer : IConsumer<ReservationRequestedEvent>
{
    private readonly TourDbContext _db;
    private readonly IPublishEndpoint _publish;
    private readonly IConnectionMultiplexer _redis;

    public ReservationRequestedConsumer(TourDbContext db, IPublishEndpoint publish, IConnectionMultiplexer redis)
    {
        _db = db;
        _publish = publish;
        _redis = redis;
    }

    public async Task Consume(ConsumeContext<ReservationRequestedEvent> context)
    {
        var msg = context.Message;

        var deal = await _db.Deals.FindAsync(msg.DealId);

        if (deal is null || deal.Status != DealStatus.Active || deal.AvailableSlots < msg.GuestCount)
        {
            var reason = deal is null ? "Deal bulunamadı."
                : deal.Status != DealStatus.Active ? $"Deal aktif değil: {deal.Status}"
                : $"Yeterli slot yok. İstenen: {msg.GuestCount}, Mevcut: {deal.AvailableSlots}";

            await _publish.Publish(new ReservationRejectedEvent(
                Guid.NewGuid(), DateTime.UtcNow, msg.ReservationId, reason));

            return;
        }

        deal.AvailableSlots -= msg.GuestCount;
        if (deal.AvailableSlots == 0)
            deal.Status = DealStatus.FullyBooked;

        await _db.SaveChangesAsync();

        await _redis.GetDatabase().KeyDeleteAsync("active-deals");

        await _publish.Publish(new ReservationConfirmedEvent(
            Guid.NewGuid(), DateTime.UtcNow, msg.ReservationId, msg.DealId));
    }
}
