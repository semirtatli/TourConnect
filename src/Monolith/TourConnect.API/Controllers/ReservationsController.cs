using MediatR;
using Microsoft.AspNetCore.Mvc;
using TourConnect.Application.Reservations;

namespace TourConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _mediator.Send(new GetReservationsQuery()));

    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}
