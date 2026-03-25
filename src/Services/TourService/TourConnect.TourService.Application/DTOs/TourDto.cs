namespace TourConnect.TourService.Application.DTOs;

public record TourDto(Guid Id, Guid OperatorId, string Title, string Description, string Category, int DurationInHours, decimal BasePrice, DateTime CreatedAt, string? OperatorName);

public record CreateTourDto(Guid OperatorId, string Title, string Description, string Category, int DurationInHours, decimal BasePrice);
