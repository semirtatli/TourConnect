namespace TourConnect.TourService.Application.DTOs;

public record OperatorDto(Guid Id, string Name, string Phone, string Location, DateTime CreatedAt);

public record CreateOperatorDto(string Name, string Phone, string Location);
