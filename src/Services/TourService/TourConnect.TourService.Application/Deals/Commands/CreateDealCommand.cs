using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.TourService.Application.DTOs;
using TourConnect.TourService.Application.Interfaces;
using TourConnect.TourService.Domain.Entities;

namespace TourConnect.TourService.Application.Deals.Commands;

public record CreateDealCommand(CreateDealDto Dto) : IRequest<DealDto?>;

public class CreateDealHandler : IRequestHandler<CreateDealCommand, DealDto?>
{
    private readonly ITourDbContext _db;

    public CreateDealHandler(ITourDbContext db) => _db = db;

    public async Task<DealDto?> Handle(CreateDealCommand request, CancellationToken cancellationToken)
    {
        var tour = await _db.Tours.FindAsync([request.Dto.TourId], cancellationToken);
        if (tour is null) return null;

        var deal = new Deal
        {
            Id = Guid.NewGuid(),
            TourId = request.Dto.TourId,
            OperatorId = tour.OperatorId,
            AvailableSlots = request.Dto.AvailableSlots,
            OriginalPrice = request.Dto.OriginalPrice,
            DiscountedPrice = request.Dto.DiscountedPrice,
            ExpiresAt = request.Dto.ExpiresAt,
            Status = DealStatus.Active
        };

        _db.Deals.Add(deal);
        await _db.SaveChangesAsync(cancellationToken);

        return new DealDto(deal.Id, deal.TourId, deal.OperatorId, deal.AvailableSlots, deal.OriginalPrice, deal.DiscountedPrice, deal.ExpiresAt, deal.Status.ToString(), deal.CreatedAt, null);
    }
}
