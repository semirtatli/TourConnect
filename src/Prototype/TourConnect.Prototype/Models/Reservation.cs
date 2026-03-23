namespace TourConnect.Prototype.Models;

public enum ReservationStatus
{
    Confirmed, // 0
    Cancelled  // 1
}

public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DealId { get; set; }
    public Deal Deal { get; set; } = null!;
    public Guid PartnerId { get; set; }
    public Partner Partner { get; set; } = null!;
    public string GuestName { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
