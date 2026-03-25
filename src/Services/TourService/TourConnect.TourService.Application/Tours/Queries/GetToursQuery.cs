using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.TourService.Application.DTOs;
using TourConnect.TourService.Application.Interfaces;

namespace TourConnect.TourService.Application.Tours.Queries;

public record GetToursQuery : IRequest<List<TourDto>>;

public class GetToursHandler : IRequestHandler<GetToursQuery, List<TourDto>>
{
    private readonly ITourDbContext _db;

    public GetToursHandler(ITourDbContext db) => _db = db;

    public async Task<List<TourDto>> Handle(GetToursQuery request, CancellationToken cancellationToken)
    {
        return await _db.Tours
            .Include(t => t.Operator)
            .Select(t => new TourDto(t.Id, t.OperatorId, t.Title, t.Description, t.Category, t.DurationInHours, t.BasePrice, t.CreatedAt, t.Operator.Name))
            .ToListAsync(cancellationToken);
    }
}
