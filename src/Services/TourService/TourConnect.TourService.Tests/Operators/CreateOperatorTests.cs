using FluentAssertions;
using TourConnect.TourService.Application.DTOs;
using TourConnect.TourService.Application.Operators.Commands;

namespace TourConnect.TourService.Tests.Operators;

public class CreateOperatorTests
{
    [Fact]
    public async Task Handle_ValidDto_CreatesOperatorAndReturnsDto()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new CreateOperatorHandler(db);

        var result = await handler.Handle(
            new CreateOperatorCommand(new CreateOperatorDto("Test Operator", "+90 555 111 2233", "Istanbul")),
            CancellationToken.None);

        result.Name.Should().Be("Test Operator");
        result.Phone.Should().Be("+90 555 111 2233");
        result.Location.Should().Be("Istanbul");
        result.Id.Should().NotBeEmpty();
        db.Operators.Should().HaveCount(1);
    }

    [Fact]
    public void Validator_EmptyName_Fails()
    {
        var validator = new CreateOperatorValidator();
        var result = validator.Validate(new CreateOperatorCommand(new CreateOperatorDto("", "+90 555", "Istanbul")));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Name"));
    }

    [Fact]
    public void Validator_ValidDto_Passes()
    {
        var validator = new CreateOperatorValidator();
        var result = validator.Validate(new CreateOperatorCommand(new CreateOperatorDto("Op", "+90 555", "Istanbul")));
        result.IsValid.Should().BeTrue();
    }
}
