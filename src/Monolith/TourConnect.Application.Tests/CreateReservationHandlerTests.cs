using TourConnect.Application.Reservations;
using TourConnect.Domain.Entities;

namespace TourConnect.Application.Tests;

// CreateReservationHandler en kritik iş kurallarını içerir:
// - Deal aktif mi?
// - Partner var mı?
// - Yeterli slot var mı?
// - Slot sıfırlanınca deal FullyBooked olur mu?
public class CreateReservationHandlerTests
{
    private readonly CreateReservationHandler _handler;
    private readonly TestDbContext _db;

    public CreateReservationHandlerTests()
    {
        // Her test sınıfı kendi izole DB'sini alır
        _db = TestDbContext.Create(nameof(CreateReservationHandlerTests));
        _handler = new CreateReservationHandler(_db);
    }

    // --- Yardımcı metotlar ---

    private async Task<Deal> CreateActiveDealAsync(int slots = 5)
    {
        var deal = new Deal
        {
            Id = Guid.NewGuid(),
            TourId = Guid.NewGuid(),
            OperatorId = Guid.NewGuid(),
            AvailableSlots = slots,
            OriginalPrice = 1000,
            DiscountedPrice = 800,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Status = DealStatus.Active
        };
        _db.Deals.Add(deal);
        await _db.SaveChangesAsync();
        return deal;
    }

    private async Task<Partner> CreatePartnerAsync()
    {
        var partner = new Partner
        {
            Id = Guid.NewGuid(),
            Name = "Test Otel",
            ContactEmail = "test@otel.com",
            Location = "İstanbul"
        };
        _db.Partners.Add(partner);
        await _db.SaveChangesAsync();
        return partner;
    }

    // --- Testler ---

    [Fact]
    public async Task Handle_ValidCommand_CreatesReservationAndDecrementsSlots()
    {
        var deal = await CreateActiveDealAsync(slots: 5);
        var partner = await CreatePartnerAsync();
        var cmd = new CreateReservationCommand(deal.Id, partner.Id, "Ali Veli", GuestCount: 2);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.Equal(ReservationStatus.Confirmed, result.Status);
        Assert.Equal(cmd.GuestName, result.GuestName);
        Assert.Equal(cmd.GuestCount, result.GuestCount);
        Assert.Equal(3, deal.AvailableSlots); // 5 - 2 = 3
    }

    [Fact]
    public async Task Handle_LastSlotTaken_DealBecomesFullyBooked()
    {
        // Son 2 slotu 2 kişilik rezervasyonla tüketiyoruz
        var deal = await CreateActiveDealAsync(slots: 2);
        var partner = await CreatePartnerAsync();
        var cmd = new CreateReservationCommand(deal.Id, partner.Id, "Ali Veli", GuestCount: 2);

        await _handler.Handle(cmd, CancellationToken.None);

        Assert.Equal(0, deal.AvailableSlots);
        Assert.Equal(DealStatus.FullyBooked, deal.Status);
    }

    [Fact]
    public async Task Handle_DealNotFound_ThrowsKeyNotFoundException()
    {
        var partner = await CreatePartnerAsync();
        var cmd = new CreateReservationCommand(Guid.NewGuid(), partner.Id, "Ali Veli", GuestCount: 1);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DealNotActive_ThrowsInvalidOperationException()
    {
        var deal = await CreateActiveDealAsync();
        deal.Status = DealStatus.Expired;
        await _db.SaveChangesAsync();
        var partner = await CreatePartnerAsync();
        var cmd = new CreateReservationCommand(deal.Id, partner.Id, "Ali Veli", GuestCount: 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PartnerNotFound_ThrowsKeyNotFoundException()
    {
        var deal = await CreateActiveDealAsync();
        var cmd = new CreateReservationCommand(deal.Id, Guid.NewGuid(), "Ali Veli", GuestCount: 1);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NotEnoughSlots_ThrowsInvalidOperationException()
    {
        var deal = await CreateActiveDealAsync(slots: 2);
        var partner = await CreatePartnerAsync();
        var cmd = new CreateReservationCommand(deal.Id, partner.Id, "Ali Veli", GuestCount: 5);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }
}
