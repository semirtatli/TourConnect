using MediatR;
using Microsoft.AspNetCore.Mvc;
using TourConnect.Application.Tours;

namespace TourConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly IMediator _mediator;

    public ToursController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _mediator.Send(new GetToursQuery()));

    [HttpPost]
    public async Task<IActionResult> Create(CreateTourCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}
