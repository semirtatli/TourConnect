using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.MatchingService.Application.DTOs;
using TourConnect.MatchingService.Application.Interfaces;

namespace TourConnect.MatchingService.Application.Reservations.Queries;

public record GetReservationByIdQuery(Guid Id) : IRequest<ReservationDto?>;

public class GetReservationByIdHandler : IRequestHandler<GetReservationByIdQuery, ReservationDto?>
{
    private readonly IMatchingDbContext _db;

    public GetReservationByIdHandler(IMatchingDbContext db) => _db = db;

    public async Task<ReservationDto?> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        return await _db.Reservations
            .Where(r => r.Id == request.Id)
            .Include(r => r.Partner)
            .Select(r => new ReservationDto(r.Id, r.DealId, r.PartnerId, r.GuestName, r.GuestCount, r.Status.ToString(), r.RejectionReason, r.CreatedAt, r.Partner.Name))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
