namespace TourConnect.Domain.Entities;

public class Tour
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OperatorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TourCategory Category { get; set; }
    public int DurationInHours { get; set; }
    public decimal BasePrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Operator Operator { get; set; } = null!;
    public ICollection<Deal> Deals { get; set; } = new List<Deal>();
}

public enum TourCategory
{
    BoatTour,
    Safari,
    Diving,
    Cultural,
    Adventure,
    Food
}
