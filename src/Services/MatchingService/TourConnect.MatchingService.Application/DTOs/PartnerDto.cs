namespace TourConnect.MatchingService.Application.DTOs;

public record PartnerDto(Guid Id, string Name, string ContactEmail, string Location, DateTime CreatedAt);

public record CreatePartnerDto(string Name, string ContactEmail, string Location);
