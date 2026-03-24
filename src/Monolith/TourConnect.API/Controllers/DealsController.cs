using MediatR;
using Microsoft.AspNetCore.Mvc;
using TourConnect.Application.Deals;

namespace TourConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DealsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DealsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetActive() =>
        Ok(await _mediator.Send(new GetActiveDealsQuery()));

    [HttpPost]
    public async Task<IActionResult> Create(CreateDealCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetActive), new { id = result.Id }, result);
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelDealCommand(id));
        return Ok(result);
    }
}
