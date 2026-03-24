using TourConnect.Application.Deals;
using TourConnect.Domain.Entities;

namespace TourConnect.Application.Tests;

public class CreateDealHandlerTests
{
    private readonly CreateDealHandler _createHandler;
    private readonly CancelDealHandler _cancelHandler;
    private readonly TestDbContext _db;

    public CreateDealHandlerTests()
    {
        _db = TestDbContext.Create(nameof(CreateDealHandlerTests));
        _createHandler = new CreateDealHandler(_db);
        _cancelHandler = new CancelDealHandler(_db);
    }

    private async Task<Tour> CreateTourAsync()
    {
        var tour = new Tour
        {
            Id = Guid.NewGuid(),
            OperatorId = Guid.NewGuid(),
            Title = "Kapadokya Turu",
            Description = "Nefes kesen manzaralar",
            Category = TourCategory.Cultural,
            DurationInHours = 8,
            BasePrice = 1500
        };
        _db.Tours.Add(tour);
        await _db.SaveChangesAsync();
        return tour;
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesDealWithActiveStatus()
    {
        var tour = await CreateTourAsync();
        var cmd = new CreateDealCommand(
            tour.Id,
            AvailableSlots: 10,
            OriginalPrice: 1500,
            DiscountedPrice: 1200,
            ExpiresAt: DateTime.UtcNow.AddDays(3));

        var result = await _createHandler.Handle(cmd, CancellationToken.None);

        Assert.Equal(DealStatus.Active, result.Status);
        Assert.Equal(tour.Id, result.TourId);
        Assert.Equal(tour.OperatorId, result.OperatorId); // OperatorId tur'dan kopyalanır
        Assert.Equal(10, result.AvailableSlots);
    }

    [Fact]
    public async Task Handle_TourNotFound_ThrowsKeyNotFoundException()
    {
        var cmd = new CreateDealCommand(
            Guid.NewGuid(),
            AvailableSlots: 10,
            OriginalPrice: 1500,
            DiscountedPrice: 1200,
            ExpiresAt: DateTime.UtcNow.AddDays(3));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _createHandler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task CancelDeal_ActiveDeal_StatusBecomesCancelled()
    {
        var tour = await CreateTourAsync();
        var deal = new Deal
        {
            Id = Guid.NewGuid(),
            TourId = tour.Id,
            OperatorId = tour.OperatorId,
            AvailableSlots = 5,
            OriginalPrice = 1500,
            DiscountedPrice = 1200,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Status = DealStatus.Active
        };
        _db.Deals.Add(deal);
        await _db.SaveChangesAsync();

        var result = await _cancelHandler.Handle(new CancelDealCommand(deal.Id), CancellationToken.None);

        Assert.Equal(DealStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task CancelDeal_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        var tour = await CreateTourAsync();
        var deal = new Deal
        {
            Id = Guid.NewGuid(),
            TourId = tour.Id,
            OperatorId = tour.OperatorId,
            AvailableSlots = 5,
            OriginalPrice = 1500,
            DiscountedPrice = 1200,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Status = DealStatus.Cancelled
        };
        _db.Deals.Add(deal);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _cancelHandler.Handle(new CancelDealCommand(deal.Id), CancellationToken.None));
    }
}
