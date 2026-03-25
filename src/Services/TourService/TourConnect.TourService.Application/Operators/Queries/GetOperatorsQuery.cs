using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.TourService.Application.DTOs;
using TourConnect.TourService.Application.Interfaces;

namespace TourConnect.TourService.Application.Operators.Queries;

public record GetOperatorsQuery : IRequest<List<OperatorDto>>;

public class GetOperatorsHandler : IRequestHandler<GetOperatorsQuery, List<OperatorDto>>
{
    private readonly ITourDbContext _db;

    public GetOperatorsHandler(ITourDbContext db) => _db = db;

    public async Task<List<OperatorDto>> Handle(GetOperatorsQuery request, CancellationToken cancellationToken)
    {
        return await _db.Operators
            .Select(o => new OperatorDto(o.Id, o.Name, o.Phone, o.Location, o.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
