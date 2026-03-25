namespace TourConnect.TourService.Application.DTOs;

public record DealDto(Guid Id, Guid TourId, Guid OperatorId, int AvailableSlots, decimal OriginalPrice, decimal DiscountedPrice, DateTime ExpiresAt, string Status, DateTime CreatedAt, string? TourTitle);

public record CreateDealDto(Guid TourId, int AvailableSlots, decimal OriginalPrice, decimal DiscountedPrice, DateTime ExpiresAt);
