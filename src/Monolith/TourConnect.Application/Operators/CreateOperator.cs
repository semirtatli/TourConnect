using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.Domain.Entities;

namespace TourConnect.Application.Operators;

// Command: sistemi değiştiren işlem (POST/PUT/DELETE)
// Query:   sistemi değiştirmeyen işlem (GET)
// Bu ayrım CQRS (Command Query Responsibility Segregation) prensibidir.

// --- COMMAND ---
// IRequest<Operator> → bu command'ın sonucunda Operator döner
public record CreateOperatorCommand(string Name, string Phone, string Location) : IRequest<Operator>;

// --- VALIDATOR ---
public class CreateOperatorValidator : AbstractValidator<CreateOperatorCommand>
{
    public CreateOperatorValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Operatör adı boş olamaz.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Telefon boş olamaz.");
        RuleFor(x => x.Location).NotEmpty().WithMessage("Konum boş olamaz.");
    }
}

// --- HANDLER ---
// IRequestHandler<TCommand, TResponse> → MediatR bu sınıfa command'ı iletir
// DbContext doğrudan burada kullanıyoruz (Anemic model: iş kuralı handler'da)
public class CreateOperatorHandler : IRequestHandler<CreateOperatorCommand, Operator>
{
    private readonly IAppDbContext _db;

    public CreateOperatorHandler(IAppDbContext db) => _db = db;

    public async Task<Operator> Handle(CreateOperatorCommand cmd, CancellationToken ct)
    {
        var op = new Operator
        {
            Name = cmd.Name,
            Phone = cmd.Phone,
            Location = cmd.Location
        };

        _db.Operators.Add(op);
        await _db.SaveChangesAsync(ct);
        return op;
    }
}

// --- QUERY ---
public record GetOperatorsQuery : IRequest<List<Operator>>;

public class GetOperatorsHandler : IRequestHandler<GetOperatorsQuery, List<Operator>>
{
    private readonly IAppDbContext _db;

    public GetOperatorsHandler(IAppDbContext db) => _db = db;

    public Task<List<Operator>> Handle(GetOperatorsQuery query, CancellationToken ct) =>
        _db.Operators.ToListAsync(ct);
}
