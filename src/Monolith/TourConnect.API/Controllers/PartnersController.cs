using MediatR;
using Microsoft.AspNetCore.Mvc;
using TourConnect.Application.Partners;

namespace TourConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartnersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PartnersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _mediator.Send(new GetPartnersQuery()));

    [HttpPost]
    public async Task<IActionResult> Create(CreatePartnerCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}
