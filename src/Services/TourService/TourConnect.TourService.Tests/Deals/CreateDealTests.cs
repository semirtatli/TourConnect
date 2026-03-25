using FluentAssertions;
using TourConnect.TourService.Application.DTOs;
using TourConnect.TourService.Application.Deals.Commands;
using TourConnect.TourService.Domain.Entities;

namespace TourConnect.TourService.Tests.Deals;

public class CreateDealTests
{
    [Fact]
    public async Task Handle_ValidDto_CreatesDeal()
    {
        using var db = TestDbContextFactory.Create();
        var op = new Operator { Name = "Op", Phone = "111", Location = "A" };
        db.Operators.Add(op);
        var tour = new Tour { OperatorId = op.Id, Title = "T", Description = "D", Category = "C", DurationInHours = 5, BasePrice = 100 };
        db.Tours.Add(tour);
        await db.SaveChangesAsync();

        var handler = new CreateDealHandler(db);
        var result = await handler.Handle(
            new CreateDealCommand(new CreateDealDto(tour.Id, 10, 100, 80, DateTime.UtcNow.AddDays(7))),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.AvailableSlots.Should().Be(10);
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_InvalidTour_ReturnsNull()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new CreateDealHandler(db);
        var result = await handler.Handle(
            new CreateDealCommand(new CreateDealDto(Guid.NewGuid(), 10, 100, 80, DateTime.UtcNow.AddDays(7))),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public void Validator_ZeroSlots_Fails()
    {
        var validator = new CreateDealValidator();
        var result = validator.Validate(new CreateDealCommand(new CreateDealDto(Guid.NewGuid(), 0, 100, 80, DateTime.UtcNow.AddDays(7))));
        result.IsValid.Should().BeFalse();
    }
}
