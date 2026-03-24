using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.Domain.Entities;

namespace TourConnect.Application.Deals;

public record CreateDealCommand(
    Guid TourId,
    int AvailableSlots,
    decimal OriginalPrice,
    decimal DiscountedPrice,
    DateTime ExpiresAt) : IRequest<Deal>;

public class CreateDealValidator : AbstractValidator<CreateDealCommand>
{
    public CreateDealValidator()
    {
        RuleFor(x => x.TourId).NotEmpty().WithMessage("TourId boş olamaz.");
        RuleFor(x => x.AvailableSlots).GreaterThan(0).WithMessage("En az 1 slot olmalı.");
        RuleFor(x => x.OriginalPrice).GreaterThan(0).WithMessage("Orijinal fiyat 0'dan büyük olmalı.");
        RuleFor(x => x.DiscountedPrice)
            .GreaterThan(0).WithMessage("İndirimli fiyat 0'dan büyük olmalı.")
            .LessThan(x => x.OriginalPrice).WithMessage("İndirimli fiyat orijinal fiyattan düşük olmalı.");
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Bitiş zamanı gelecekte olmalı.");
    }
}

public class CreateDealHandler : IRequestHandler<CreateDealCommand, Deal>
{
    private readonly IAppDbContext _db;

    public CreateDealHandler(IAppDbContext db) => _db = db;

    public async Task<Deal> Handle(CreateDealCommand cmd, CancellationToken ct)
    {
        var tour = await _db.Tours.FindAsync([cmd.TourId], ct)
            ?? throw new KeyNotFoundException($"Tur bulunamadı: {cmd.TourId}");

        var deal = new Deal
        {
            TourId = cmd.TourId,
            OperatorId = tour.OperatorId,
            AvailableSlots = cmd.AvailableSlots,
            OriginalPrice = cmd.OriginalPrice,
            DiscountedPrice = cmd.DiscountedPrice,
            ExpiresAt = cmd.ExpiresAt,
            Status = DealStatus.Active
        };

        _db.Deals.Add(deal);
        await _db.SaveChangesAsync(ct);
        return deal;
    }
}

public record GetActiveDealsQuery : IRequest<List<Deal>>;

public class GetActiveDealsHandler : IRequestHandler<GetActiveDealsQuery, List<Deal>>
{
    private readonly IAppDbContext _db;

    public GetActiveDealsHandler(IAppDbContext db) => _db = db;

    public Task<List<Deal>> Handle(GetActiveDealsQuery query, CancellationToken ct) =>
        _db.Deals
            .Where(d => d.Status == DealStatus.Active)
            .Include(d => d.Tour).ThenInclude(t => t.Operator)
            .ToListAsync(ct);
}

public record CancelDealCommand(Guid Id) : IRequest<Deal>;

public class CancelDealHandler : IRequestHandler<CancelDealCommand, Deal>
{
    private readonly IAppDbContext _db;

    public CancelDealHandler(IAppDbContext db) => _db = db;

    public async Task<Deal> Handle(CancelDealCommand cmd, CancellationToken ct)
    {
        var deal = await _db.Deals.FindAsync([cmd.Id], ct)
            ?? throw new KeyNotFoundException($"Deal bulunamadı: {cmd.Id}");

        if (deal.Status != DealStatus.Active)
            throw new InvalidOperationException($"Bu deal iptal edilemez. Mevcut durum: {deal.Status}");

        deal.Status = DealStatus.Cancelled;
        await _db.SaveChangesAsync(ct);
        return deal;
    }
}
