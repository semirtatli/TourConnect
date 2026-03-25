using FluentAssertions;
using TourConnect.TourService.Application.Operators.Queries;
using TourConnect.TourService.Domain.Entities;

namespace TourConnect.TourService.Tests.Operators;

public class GetOperatorsTests
{
    [Fact]
    public async Task Handle_ReturnsAllOperators()
    {
        using var db = TestDbContextFactory.Create();
        db.Operators.AddRange(
            new Operator { Name = "Op1", Phone = "111", Location = "A" },
            new Operator { Name = "Op2", Phone = "222", Location = "B" });
        await db.SaveChangesAsync();

        var handler = new GetOperatorsHandler(db);
        var result = await handler.Handle(new GetOperatorsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_EmptyDb_ReturnsEmptyList()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetOperatorsHandler(db);
        var result = await handler.Handle(new GetOperatorsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
