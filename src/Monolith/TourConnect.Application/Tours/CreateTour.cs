using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.Domain.Entities;

namespace TourConnect.Application.Tours;

public record CreateTourCommand(
    Guid OperatorId,
    string Title,
    string Description,
    TourCategory Category,
    int DurationInHours,
    decimal BasePrice) : IRequest<Tour>;

public class CreateTourValidator : AbstractValidator<CreateTourCommand>
{
    public CreateTourValidator()
    {
        RuleFor(x => x.OperatorId).NotEmpty().WithMessage("OperatorId boş olamaz.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Tur adı boş olamaz.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Açıklama boş olamaz.");
        RuleFor(x => x.DurationInHours).GreaterThan(0).WithMessage("Süre en az 1 saat olmalı.");
        RuleFor(x => x.BasePrice).GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalı.");
    }
}

public class CreateTourHandler : IRequestHandler<CreateTourCommand, Tour>
{
    private readonly IAppDbContext _db;

    public CreateTourHandler(IAppDbContext db) => _db = db;

    public async Task<Tour> Handle(CreateTourCommand cmd, CancellationToken ct)
    {
        var operatorExists = await _db.Operators.AnyAsync(o => o.Id == cmd.OperatorId, ct);
        if (!operatorExists)
            throw new KeyNotFoundException($"Operatör bulunamadı: {cmd.OperatorId}");

        var tour = new Tour
        {
            OperatorId = cmd.OperatorId,
            Title = cmd.Title,
            Description = cmd.Description,
            Category = cmd.Category,
            DurationInHours = cmd.DurationInHours,
            BasePrice = cmd.BasePrice
        };

        _db.Tours.Add(tour);
        await _db.SaveChangesAsync(ct);
        return tour;
    }
}

public record GetToursQuery : IRequest<List<Tour>>;

public class GetToursHandler : IRequestHandler<GetToursQuery, List<Tour>>
{
    private readonly IAppDbContext _db;

    public GetToursHandler(IAppDbContext db) => _db = db;

    public Task<List<Tour>> Handle(GetToursQuery query, CancellationToken ct) =>
        _db.Tours.Include(t => t.Operator).ToListAsync(ct);
}
