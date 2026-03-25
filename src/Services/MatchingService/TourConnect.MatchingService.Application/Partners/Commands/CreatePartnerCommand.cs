using MediatR;
using TourConnect.MatchingService.Application.DTOs;
using TourConnect.MatchingService.Application.Interfaces;
using TourConnect.MatchingService.Domain.Entities;

namespace TourConnect.MatchingService.Application.Partners.Commands;

public record CreatePartnerCommand(CreatePartnerDto Dto) : IRequest<PartnerDto>;

public class CreatePartnerHandler : IRequestHandler<CreatePartnerCommand, PartnerDto>
{
    private readonly IMatchingDbContext _db;

    public CreatePartnerHandler(IMatchingDbContext db) => _db = db;

    public async Task<PartnerDto> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = new Partner
        {
            Id = Guid.NewGuid(),
            Name = request.Dto.Name,
            ContactEmail = request.Dto.ContactEmail,
            Location = request.Dto.Location
        };

        _db.Partners.Add(partner);
        await _db.SaveChangesAsync(cancellationToken);

        return new PartnerDto(partner.Id, partner.Name, partner.ContactEmail, partner.Location, partner.CreatedAt);
    }
}
