using MediatR;
using Microsoft.EntityFrameworkCore;
using TourConnect.TourService.Application.DTOs;
using TourConnect.TourService.Application.Interfaces;
using TourConnect.TourService.Domain.Entities;

namespace TourConnect.TourService.Application.Operators.Commands;

public record CreateOperatorCommand(CreateOperatorDto Dto) : IRequest<OperatorDto>;

public class CreateOperatorHandler : IRequestHandler<CreateOperatorCommand, OperatorDto>
{
    private readonly ITourDbContext _db;

    public CreateOperatorHandler(ITourDbContext db) => _db = db;

    public async Task<OperatorDto> Handle(CreateOperatorCommand request, CancellationToken cancellationToken)
    {
        var op = new Operator
        {
            Id = Guid.NewGuid(),
            Name = request.Dto.Name,
            Phone = request.Dto.Phone,
            Location = request.Dto.Location
        };

        _db.Operators.Add(op);
        await _db.SaveChangesAsync(cancellationToken);

        return new OperatorDto(op.Id, op.Name, op.Phone, op.Location, op.CreatedAt);
    }
}
