using TourConnect.Application.Deals;
using TourConnect.Application.Operators;
using TourConnect.Application.Reservations;

namespace TourConnect.Application.Tests;

// Validator testleri: iş kurallarının girdi doğrulama katmanında doğru çalıştığını kontrol eder.
// Handler'a hiç ulaşmadan hatalı girdiler reddedilmeli.
public class ValidatorTests
{
    // --- CreateOperatorValidator ---

    [Fact]
    public async Task CreateOperatorValidator_EmptyName_Fails()
    {
        var validator = new CreateOperatorValidator();
        var result = await validator.ValidateAsync(new CreateOperatorCommand("", "555", "İstanbul"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateOperatorCommand.Name));
    }

    [Fact]
    public async Task CreateOperatorValidator_ValidCommand_Passes()
    {
        var validator = new CreateOperatorValidator();
        var result = await validator.ValidateAsync(new CreateOperatorCommand("Tur A.Ş.", "555-0001", "Ankara"));
        Assert.True(result.IsValid);
    }

    // --- CreateDealValidator ---

    [Fact]
    public async Task CreateDealValidator_DiscountedPriceHigherThanOriginal_Fails()
    {
        var validator = new CreateDealValidator();
        var cmd = new CreateDealCommand(
            Guid.NewGuid(),
            AvailableSlots: 5,
            OriginalPrice: 1000,
            DiscountedPrice: 1500, // orijinalden yüksek — geçersiz
            ExpiresAt: DateTime.UtcNow.AddDays(1));

        var result = await validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDealCommand.DiscountedPrice));
    }

    [Fact]
    public async Task CreateDealValidator_ExpiresAtInPast_Fails()
    {
        var validator = new CreateDealValidator();
        var cmd = new CreateDealCommand(
            Guid.NewGuid(),
            AvailableSlots: 5,
            OriginalPrice: 1000,
            DiscountedPrice: 800,
            ExpiresAt: DateTime.UtcNow.AddDays(-1)); // geçmişte — geçersiz

        var result = await validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDealCommand.ExpiresAt));
    }

    [Fact]
    public async Task CreateDealValidator_ZeroSlots_Fails()
    {
        var validator = new CreateDealValidator();
        var cmd = new CreateDealCommand(
            Guid.NewGuid(),
            AvailableSlots: 0,
            OriginalPrice: 1000,
            DiscountedPrice: 800,
            ExpiresAt: DateTime.UtcNow.AddDays(1));

        var result = await validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDealCommand.AvailableSlots));
    }

    // --- CreateReservationValidator ---

    [Fact]
    public async Task CreateReservationValidator_ZeroGuestCount_Fails()
    {
        var validator = new CreateReservationValidator();
        var cmd = new CreateReservationCommand(Guid.NewGuid(), Guid.NewGuid(), "Ali", GuestCount: 0);

        var result = await validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReservationCommand.GuestCount));
    }

    [Fact]
    public async Task CreateReservationValidator_EmptyGuestName_Fails()
    {
        var validator = new CreateReservationValidator();
        var cmd = new CreateReservationCommand(Guid.NewGuid(), Guid.NewGuid(), "", GuestCount: 2);

        var result = await validator.ValidateAsync(cmd);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReservationCommand.GuestName));
    }
}
