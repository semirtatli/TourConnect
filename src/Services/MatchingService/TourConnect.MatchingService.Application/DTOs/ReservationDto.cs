namespace TourConnect.MatchingService.Application.DTOs;

public record ReservationDto(Guid Id, Guid DealId, Guid PartnerId, string GuestName, int GuestCount, string Status, string? RejectionReason, DateTime CreatedAt, string? PartnerName);

public record CreateReservationDto(Guid DealId, Guid PartnerId, string GuestName, int GuestCount);
