using EventBus.Messages.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using TourConnect.TourService.Data;
using TourConnect.TourService.Entities;

namespace TourConnect.TourService.Consumers;

// MassTransit bu sınıfı otomatik olarak RabbitMQ'ya bağlar.
// "ReservationRequested" kuyruğunu dinler.
// Matching Service bir rezervasyon isteği yayınladığında bu metot çalışır.
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

        // Slot yoksa veya deal aktif değilse → reddet
        if (deal is null || deal.Status != DealStatus.Active || deal.AvailableSlots < msg.GuestCount)
        {
            var reason = deal is null ? "Deal bulunamadı."
                : deal.Status != DealStatus.Active ? $"Deal aktif değil: {deal.Status}"
                : $"Yeterli slot yok. İstenen: {msg.GuestCount}, Mevcut: {deal.AvailableSlots}";

            await _publish.Publish(new ReservationRejectedEvent(
                Guid.NewGuid(), DateTime.UtcNow, msg.ReservationId, reason));

            return;
        }

        // Slot var → düş, gerekirse FullyBooked yap
        deal.AvailableSlots -= msg.GuestCount;
        if (deal.AvailableSlots == 0)
            deal.Status = DealStatus.FullyBooked;

        await _db.SaveChangesAsync();

        // Deal bilgisi değişti → Redis cache'i geçersiz kıl.
        // Yoksa GET /api/deals eski (hatalı) slot sayısını döner ve overbooking olur.
        await _redis.GetDatabase().KeyDeleteAsync("active-deals");

        // Başarılı → onayla
        await _publish.Publish(new ReservationConfirmedEvent(
            Guid.NewGuid(), DateTime.UtcNow, msg.ReservationId, msg.DealId));
    }
}
