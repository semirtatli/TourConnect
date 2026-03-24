namespace TourConnect.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DealId { get; set; }
    public Guid PartnerId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Deal Deal { get; set; } = null!;
    public Partner Partner { get; set; } = null!;
}

public enum ReservationStatus
{
    Pending,
    Confirmed,
    Cancelled
}
