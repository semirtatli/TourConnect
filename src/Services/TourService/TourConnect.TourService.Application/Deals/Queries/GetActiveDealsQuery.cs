using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.TourService.Application.DTOs;
using TourConnect.TourService.Application.Interfaces;
using TourConnect.TourService.Domain.Entities;

namespace TourConnect.TourService.Application.Deals.Queries;

public record GetActiveDealsQuery : IRequest<List<DealDto>>;

public class GetActiveDealsHandler : IRequestHandler<GetActiveDealsQuery, List<DealDto>>
{
    private readonly ITourDbContext _db;

    public GetActiveDealsHandler(ITourDbContext db) => _db = db;

    public async Task<List<DealDto>> Handle(GetActiveDealsQuery request, CancellationToken cancellationToken)
    {
        return await _db.Deals
            .Where(d => d.Status == DealStatus.Active)
            .Include(d => d.Tour)
            .Select(d => new DealDto(d.Id, d.TourId, d.OperatorId, d.AvailableSlots, d.OriginalPrice, d.DiscountedPrice, d.ExpiresAt, d.Status.ToString(), d.CreatedAt, d.Tour.Title))
            .ToListAsync(cancellationToken);
    }
}
