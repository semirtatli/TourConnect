using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EventBus.Messages.Events;
using TourConnect.MatchingService.Application.DTOs;
using TourConnect.MatchingService.Application.Interfaces;
using TourConnect.MatchingService.Domain.Entities;

namespace TourConnect.MatchingService.Application.Reservations.Commands;

public record CreateReservationCommand(CreateReservationDto Dto) : IRequest<ReservationDto?>;

public class CreateReservationHandler : IRequestHandler<CreateReservationCommand, ReservationDto?>
{
    private readonly IMatchingDbContext _db;
    private readonly IPublishEndpoint _publish;

    public CreateReservationHandler(IMatchingDbContext db, IPublishEndpoint publish)
    {
        _db = db;
        _publish = publish;
    }

    public async Task<ReservationDto?> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var partnerExists = await _db.Partners.AnyAsync(p => p.Id == request.Dto.PartnerId, cancellationToken);
        if (!partnerExists) return null;

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            DealId = request.Dto.DealId,
            PartnerId = request.Dto.PartnerId,
            GuestName = request.Dto.GuestName,
            GuestCount = request.Dto.GuestCount,
            Status = ReservationStatus.Pending
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync(cancellationToken);

        await _publish.Publish(new ReservationRequestedEvent(
            EventId: Guid.NewGuid(),
            CreatedAt: DateTime.UtcNow,
            ReservationId: reservation.Id,
            DealId: reservation.DealId,
            PartnerId: reservation.PartnerId,
            GuestName: reservation.GuestName,
            GuestCount: reservation.GuestCount), cancellationToken);

        return new ReservationDto(reservation.Id, reservation.DealId, reservation.PartnerId, reservation.GuestName, reservation.GuestCount, reservation.Status.ToString(), reservation.RejectionReason, reservation.CreatedAt, null);
    }
}
