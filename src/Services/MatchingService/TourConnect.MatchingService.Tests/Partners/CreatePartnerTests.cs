using FluentAssertions;
using TourConnect.MatchingService.Application.DTOs;
using TourConnect.MatchingService.Application.Partners.Commands;

namespace TourConnect.MatchingService.Tests.Partners;

public class CreatePartnerTests
{
    [Fact]
    public async Task Handle_ValidDto_CreatesPartner()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new CreatePartnerHandler(db);

        var result = await handler.Handle(
            new CreatePartnerCommand(new CreatePartnerDto("Test Hotel", "test@hotel.com", "Bodrum")),
            CancellationToken.None);

        result.Name.Should().Be("Test Hotel");
        result.ContactEmail.Should().Be("test@hotel.com");
        result.Id.Should().NotBeEmpty();
        db.Partners.Should().HaveCount(1);
    }

    [Fact]
    public void Validator_EmptyName_Fails()
    {
        var validator = new CreatePartnerValidator();
        var result = validator.Validate(new CreatePartnerCommand(new CreatePartnerDto("", "test@hotel.com", "Bodrum")));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_InvalidEmail_Fails()
    {
        var validator = new CreatePartnerValidator();
        var result = validator.Validate(new CreatePartnerCommand(new CreatePartnerDto("Hotel", "not-email", "Bodrum")));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_ValidDto_Passes()
    {
        var validator = new CreatePartnerValidator();
        var result = validator.Validate(new CreatePartnerCommand(new CreatePartnerDto("Hotel", "a@b.com", "Bodrum")));
        result.IsValid.Should().BeTrue();
    }
}
