namespace EventBus.Messages.Events;

// Tour Service yayınlar → Matching Service tüketir
// Yeterli slot yoksa: Reservation.Status = Rejected
public record ReservationRejectedEvent(
    Guid EventId,
    DateTime CreatedAt,
    Guid ReservationId,
    string Reason
);
