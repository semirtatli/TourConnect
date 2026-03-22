namespace EventBus.Messages.Events;

// Matching Service yayınlar → Tour Service tüketir
// Otel partneri rezervasyon oluşturduğunda tetiklenir
public record ReservationRequestedEvent(
    Guid EventId,
    DateTime CreatedAt,
    Guid ReservationId,
    Guid DealId,
    Guid PartnerId,
    string GuestName,
    int GuestCount
);
