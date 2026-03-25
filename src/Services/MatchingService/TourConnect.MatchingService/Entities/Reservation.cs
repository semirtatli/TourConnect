namespace TourConnect.MatchingService.Entities;

public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DealId { get; set; }
    public Guid PartnerId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Partner Partner { get; set; } = null!;
}

// FAZ 4'teki fark: Pending durumu var.
// Rezervasyon önce Pending olarak kaydedilir, Tour Service'in cevabı gelince güncellenir.
public enum ReservationStatus { Pending, Confirmed, Rejected }
