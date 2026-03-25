using FluentAssertions;
using TourConnect.MatchingService.Application.Partners.Queries;
using TourConnect.MatchingService.Domain.Entities;

namespace TourConnect.MatchingService.Tests.Partners;

public class GetPartnersTests
{
    [Fact]
    public async Task Handle_ReturnsAllPartners()
    {
        using var db = TestDbContextFactory.Create();
        db.Partners.AddRange(
            new Partner { Name = "Hotel1", ContactEmail = "a@a.com", Location = "A" },
            new Partner { Name = "Hotel2", ContactEmail = "b@b.com", Location = "B" });
        await db.SaveChangesAsync();

        var handler = new GetPartnersHandler(db);
        var result = await handler.Handle(new GetPartnersQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }
}
