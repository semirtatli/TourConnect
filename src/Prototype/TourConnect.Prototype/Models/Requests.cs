namespace TourConnect.Prototype.Models;

// Request DTO'ları: POST endpoint'lerine gelen JSON'un şekli.
// "record" → immutable, equality otomatik, concise syntax.
// Faz 2'de her request kendi dosyasına taşınacak.

public record CreateOperatorRequest(string Name, string Phone, string Location);

public record CreateTourRequest(
    Guid OperatorId,
    string Title,
    string Description,
    TourCategory Category,
    int DurationInHours,
    decimal BasePrice
);

public record CreateDealRequest(
    Guid TourId,
    int AvailableSlots,
    decimal OriginalPrice,
    decimal DiscountedPrice,
    DateTime ExpiresAt
);

public record CreatePartnerRequest(string Name, string ContactEmail, string Location);

public record CreateReservationRequest(
    Guid DealId,
    Guid PartnerId,
    string GuestName,
    int GuestCount
);
