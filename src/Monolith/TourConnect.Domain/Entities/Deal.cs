namespace TourConnect.Domain.Entities;

public class Deal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TourId { get; set; }
    public Guid OperatorId { get; set; }
    public int AvailableSlots { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DealStatus Status { get; set; } = DealStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tour Tour { get; set; } = null!;
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

public enum DealStatus
{
    Active,
    Expired,
    FullyBooked,
    Cancelled
}
