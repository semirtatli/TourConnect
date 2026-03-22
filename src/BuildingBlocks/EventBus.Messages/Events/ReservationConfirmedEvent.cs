namespace EventBus.Messages.Events;

// Tour Service yayınlar → Matching Service tüketir
// Slot başarıyla düşürüldüğünde: Reservation.Status = Confirmed
public record ReservationConfirmedEvent(
    Guid EventId,
    DateTime CreatedAt,
    Guid ReservationId,
    Guid DealId
);
