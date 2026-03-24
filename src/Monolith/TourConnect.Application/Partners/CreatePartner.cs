using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.Domain.Entities;

namespace TourConnect.Application.Partners;

public record CreatePartnerCommand(string Name, string ContactEmail, string Location) : IRequest<Partner>;

public class CreatePartnerValidator : AbstractValidator<CreatePartnerCommand>
{
    public CreatePartnerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Otel adı boş olamaz.");
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");
        RuleFor(x => x.Location).NotEmpty().WithMessage("Konum boş olamaz.");
    }
}

public class CreatePartnerHandler : IRequestHandler<CreatePartnerCommand, Partner>
{
    private readonly IAppDbContext _db;

    public CreatePartnerHandler(IAppDbContext db) => _db = db;

    public async Task<Partner> Handle(CreatePartnerCommand cmd, CancellationToken ct)
    {
        var partner = new Partner
        {
            Name = cmd.Name,
            ContactEmail = cmd.ContactEmail,
            Location = cmd.Location
        };

        _db.Partners.Add(partner);
        await _db.SaveChangesAsync(ct);
        return partner;
    }
}

public record GetPartnersQuery : IRequest<List<Partner>>;

public class GetPartnersHandler : IRequestHandler<GetPartnersQuery, List<Partner>>
{
    private readonly IAppDbContext _db;

    public GetPartnersHandler(IAppDbContext db) => _db = db;

    public Task<List<Partner>> Handle(GetPartnersQuery query, CancellationToken ct) =>
        _db.Partners.ToListAsync(ct);
}
