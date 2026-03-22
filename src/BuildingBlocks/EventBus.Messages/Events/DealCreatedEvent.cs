namespace EventBus.Messages.Events;

// Tour Service yayınlar → Matching Service tüketir
// Yeni bir last-minute fırsat oluşturulduğunda tetiklenir
public record DealCreatedEvent(
    Guid EventId,
    DateTime CreatedAt,
    Guid DealId,
    Guid TourId,
    Guid OperatorId,
    string TourTitle,
    int AvailableSlots,
    decimal OriginalPrice,
    decimal DiscountedPrice,
    DateTime ExpiresAt
);
