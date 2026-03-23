namespace TourConnect.Prototype.Models;

public enum TourCategory
{
    BoatTour,   // 0
    Safari,     // 1
    Diving,     // 2
    Cultural,   // 3
    Adventure,  // 4
    Food        // 5
}

public class Tour
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OperatorId { get; set; }
    public Operator Operator { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TourCategory Category { get; set; }
    public int DurationInHours { get; set; }
    public decimal BasePrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
