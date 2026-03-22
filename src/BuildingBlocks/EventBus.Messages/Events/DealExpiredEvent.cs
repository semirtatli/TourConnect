namespace EventBus.Messages.Events;

// Tour Service yayınlar → Matching Service tüketir
// DealExpiryCheckerService süresi dolmuş deal'i Expired yapınca tetiklenir
public record DealExpiredEvent(
    Guid EventId,
    DateTime CreatedAt,
    Guid DealId
);
