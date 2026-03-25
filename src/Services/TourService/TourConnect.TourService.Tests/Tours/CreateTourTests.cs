using FluentAssertions;
using TourConnect.TourService.Application.DTOs;
using TourConnect.TourService.Application.Tours.Commands;
using TourConnect.TourService.Domain.Entities;

namespace TourConnect.TourService.Tests.Tours;

public class CreateTourTests
{
    [Fact]
    public async Task Handle_ValidDto_CreatesTour()
    {
        using var db = TestDbContextFactory.Create();
        var op = new Operator { Name = "Op", Phone = "111", Location = "A" };
        db.Operators.Add(op);
        await db.SaveChangesAsync();

        var handler = new CreateTourHandler(db);
        var result = await handler.Handle(
            new CreateTourCommand(new CreateTourDto(op.Id, "Tour1", "Desc", "Cat", 5, 100)),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Tour1");
        result.OperatorId.Should().Be(op.Id);
    }

    [Fact]
    public async Task Handle_InvalidOperator_ReturnsNull()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new CreateTourHandler(db);
        var result = await handler.Handle(
            new CreateTourCommand(new CreateTourDto(Guid.NewGuid(), "Tour1", "Desc", "Cat", 5, 100)),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public void Validator_EmptyTitle_Fails()
    {
        var validator = new CreateTourValidator();
        var result = validator.Validate(new CreateTourCommand(new CreateTourDto(Guid.NewGuid(), "", "Desc", "Cat", 5, 100)));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_ZeroDuration_Fails()
    {
        var validator = new CreateTourValidator();
        var result = validator.Validate(new CreateTourCommand(new CreateTourDto(Guid.NewGuid(), "T", "D", "C", 0, 100)));
        result.IsValid.Should().BeFalse();
    }
}
