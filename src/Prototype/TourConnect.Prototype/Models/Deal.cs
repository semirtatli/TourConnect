namespace TourConnect.Prototype.Models;

public enum DealStatus
{
    Active,      // 0
    Expired,     // 1
    FullyBooked, // 2
    Cancelled    // 3
}

public class Deal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TourId { get; set; }
    public Tour Tour { get; set; } = null!;
    public Guid OperatorId { get; set; }
    public int AvailableSlots { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DealStatus Status { get; set; } = DealStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
