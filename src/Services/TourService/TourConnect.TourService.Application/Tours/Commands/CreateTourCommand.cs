using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.TourService.Application.DTOs;
using TourConnect.TourService.Application.Interfaces;
using TourConnect.TourService.Domain.Entities;

namespace TourConnect.TourService.Application.Tours.Commands;

public record CreateTourCommand(CreateTourDto Dto) : IRequest<TourDto?>;

public class CreateTourHandler : IRequestHandler<CreateTourCommand, TourDto?>
{
    private readonly ITourDbContext _db;

    public CreateTourHandler(ITourDbContext db) => _db = db;

    public async Task<TourDto?> Handle(CreateTourCommand request, CancellationToken cancellationToken)
    {
        var operatorExists = await _db.Operators.AnyAsync(o => o.Id == request.Dto.OperatorId, cancellationToken);
        if (!operatorExists) return null;

        var tour = new Tour
        {
            Id = Guid.NewGuid(),
            OperatorId = request.Dto.OperatorId,
            Title = request.Dto.Title,
            Description = request.Dto.Description,
            Category = request.Dto.Category,
            DurationInHours = request.Dto.DurationInHours,
            BasePrice = request.Dto.BasePrice
        };

        _db.Tours.Add(tour);
        await _db.SaveChangesAsync(cancellationToken);

        return new TourDto(tour.Id, tour.OperatorId, tour.Title, tour.Description, tour.Category, tour.DurationInHours, tour.BasePrice, tour.CreatedAt, null);
    }
}
