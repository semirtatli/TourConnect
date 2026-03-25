using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.MatchingService.Application.DTOs;
using TourConnect.MatchingService.Application.Interfaces;

namespace TourConnect.MatchingService.Application.Partners.Queries;

public record GetPartnersQuery : IRequest<List<PartnerDto>>;

public class GetPartnersHandler : IRequestHandler<GetPartnersQuery, List<PartnerDto>>
{
    private readonly IMatchingDbContext _db;

    public GetPartnersHandler(IMatchingDbContext db) => _db = db;

    public async Task<List<PartnerDto>> Handle(GetPartnersQuery request, CancellationToken cancellationToken)
    {
        return await _db.Partners
            .Select(p => new PartnerDto(p.Id, p.Name, p.ContactEmail, p.Location, p.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
