namespace EventBus.Messages.Events;

// Tour Service yayınlar → Matching Service tüketir
// Rezervasyon onaylandığında slot azaldığında tetiklenir
public record DealSlotsUpdatedEvent(
    Guid EventId,
    DateTime CreatedAt,
    Guid DealId,
    int RemainingSlots
);
