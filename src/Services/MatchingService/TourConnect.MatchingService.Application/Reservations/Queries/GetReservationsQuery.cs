using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.MatchingService.Application.DTOs;
using TourConnect.MatchingService.Application.Interfaces;

namespace TourConnect.MatchingService.Application.Reservations.Queries;

public record GetReservationsQuery : IRequest<List<ReservationDto>>;

public class GetReservationsHandler : IRequestHandler<GetReservationsQuery, List<ReservationDto>>
{
    private readonly IMatchingDbContext _db;

    public GetReservationsHandler(IMatchingDbContext db) => _db = db;

    public async Task<List<ReservationDto>> Handle(GetReservationsQuery request, CancellationToken cancellationToken)
    {
        return await _db.Reservations
            .Include(r => r.Partner)
            .Select(r => new ReservationDto(r.Id, r.DealId, r.PartnerId, r.GuestName, r.GuestCount, r.Status.ToString(), r.RejectionReason, r.CreatedAt, r.Partner.Name))
            .ToListAsync(cancellationToken);
    }
}
