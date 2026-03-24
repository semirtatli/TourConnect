using MediatR;
using Microsoft.AspNetCore.Mvc;
using TourConnect.Application.Operators;

namespace TourConnect.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OperatorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OperatorsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _mediator.Send(new GetOperatorsQuery()));

    [HttpPost]
    public async Task<IActionResult> Create(CreateOperatorCommand cmd)
    {
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }
}
