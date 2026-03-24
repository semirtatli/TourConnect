using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.Domain.Entities;

namespace TourConnect.Application.Reservations;

public record CreateReservationCommand(
    Guid DealId,
    Guid PartnerId,
    string GuestName,
    int GuestCount) : IRequest<Reservation>;

public class CreateReservationValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.DealId).NotEmpty().WithMessage("DealId boş olamaz.");
        RuleFor(x => x.PartnerId).NotEmpty().WithMessage("PartnerId boş olamaz.");
        RuleFor(x => x.GuestName).NotEmpty().WithMessage("Misafir adı boş olamaz.");
        RuleFor(x => x.GuestCount).GreaterThan(0).WithMessage("Misafir sayısı en az 1 olmalı.");
    }
}

public class CreateReservationHandler : IRequestHandler<CreateReservationCommand, Reservation>
{
    private readonly IAppDbContext _db;

    public CreateReservationHandler(IAppDbContext db) => _db = db;

    public async Task<Reservation> Handle(CreateReservationCommand cmd, CancellationToken ct)
    {
        var deal = await _db.Deals.FindAsync([cmd.DealId], ct)
            ?? throw new KeyNotFoundException($"Deal bulunamadı: {cmd.DealId}");

        if (deal.Status != DealStatus.Active)
            throw new InvalidOperationException($"Bu deal rezervasyon kabul etmiyor. Durum: {deal.Status}");

        var partnerExists = await _db.Partners.AnyAsync(p => p.Id == cmd.PartnerId, ct);
        if (!partnerExists)
            throw new KeyNotFoundException($"Partner bulunamadı: {cmd.PartnerId}");

        if (deal.AvailableSlots < cmd.GuestCount)
            throw new InvalidOperationException($"Yeterli slot yok. İstenen: {cmd.GuestCount}, Mevcut: {deal.AvailableSlots}");

        deal.AvailableSlots -= cmd.GuestCount;
        if (deal.AvailableSlots == 0)
            deal.Status = DealStatus.FullyBooked;

        var reservation = new Reservation
        {
            DealId = cmd.DealId,
            PartnerId = cmd.PartnerId,
            GuestName = cmd.GuestName,
            GuestCount = cmd.GuestCount,
            Status = ReservationStatus.Confirmed
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync(ct);
        return reservation;
    }
}

public record GetReservationsQuery : IRequest<List<Reservation>>;

public class GetReservationsHandler : IRequestHandler<GetReservationsQuery, List<Reservation>>
{
    private readonly IAppDbContext _db;

    public GetReservationsHandler(IAppDbContext db) => _db = db;

    public Task<List<Reservation>> Handle(GetReservationsQuery query, CancellationToken ct) =>
        _db.Reservations
            .Include(r => r.Deal).ThenInclude(d => d.Tour)
            .Include(r => r.Partner)
            .ToListAsync(ct);
}
