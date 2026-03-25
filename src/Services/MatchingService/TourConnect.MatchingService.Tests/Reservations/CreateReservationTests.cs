using FluentAssertions;
using MassTransit;
using NSubstitute;
using TourConnect.MatchingService.Application.DTOs;
using TourConnect.MatchingService.Application.Reservations.Commands;
using TourConnect.MatchingService.Domain.Entities;

namespace TourConnect.MatchingService.Tests.Reservations;

public class CreateReservationTests
{
    [Fact]
    public async Task Handle_ValidDto_CreatesReservationAndPublishesEvent()
    {
        using var db = TestDbContextFactory.Create();
        var partner = new Partner { Name = "Hotel", ContactEmail = "a@a.com", Location = "A" };
        db.Partners.Add(partner);
        await db.SaveChangesAsync();

        var publish = Substitute.For<IPublishEndpoint>();
        var handler = new CreateReservationHandler(db, publish);

        var result = await handler.Handle(
            new CreateReservationCommand(new CreateReservationDto(Guid.NewGuid(), partner.Id, "Ali Yılmaz", 3)),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be("Pending");
        result.GuestName.Should().Be("Ali Yılmaz");
        db.Reservations.Should().HaveCount(1);
        await publish.Received(1).Publish(Arg.Any<EventBus.Messages.Events.ReservationRequestedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidPartner_ReturnsNull()
    {
        using var db = TestDbContextFactory.Create();
        var publish = Substitute.For<IPublishEndpoint>();
        var handler = new CreateReservationHandler(db, publish);

        var result = await handler.Handle(
            new CreateReservationCommand(new CreateReservationDto(Guid.NewGuid(), Guid.NewGuid(), "Ali", 2)),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public void Validator_EmptyGuestName_Fails()
    {
        var validator = new CreateReservationValidator();
        var result = validator.Validate(new CreateReservationCommand(new CreateReservationDto(Guid.NewGuid(), Guid.NewGuid(), "", 2)));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_ZeroGuestCount_Fails()
    {
        var validator = new CreateReservationValidator();
        var result = validator.Validate(new CreateReservationCommand(new CreateReservationDto(Guid.NewGuid(), Guid.NewGuid(), "Ali", 0)));
        result.IsValid.Should().BeFalse();
    }
}
