namespace TourConnect.Prototype.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ToursController : ControllerBase
{
    private readonly AppDbContext _db;

    public ToursController(AppDbContext db) => _db = db;

    // GET /api/tours
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Tours.Include(t => t.Operator).ToListAsync());

    // POST /api/tours
    [HttpPost]
    public async Task<IActionResult> Create(CreateTourRequest request)
    {
        var operatorExists = await _db.Operators.FindAsync(request.OperatorId);
        if (operatorExists is null)
            return NotFound($"Operatör bulunamadı: {request.OperatorId}");

        var tour = new Tour
        {
            OperatorId = request.OperatorId,
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            DurationInHours = request.DurationInHours,
            BasePrice = request.BasePrice
        };

        _db.Tours.Add(tour);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = tour.Id }, tour);
    }
}
