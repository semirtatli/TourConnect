namespace TourConnect.Prototype.Controllers;

// [ApiController] → otomatik model validation, 400 yanıtları, binding hataları
// [Route("api/[controller]")] → [controller] yerine sınıf adının "Controller" kısmı çıkarılır
//   → OperatorsController → /api/operators
[ApiController]
[Route("api/[controller]")]
public class OperatorsController : ControllerBase
{
    // Constructor injection: AppDbContext, DI container tarafından otomatik verilir.
    // Faz 0'da endpoint parametresinde geliyordu, şimdi constructor'da alıyoruz.
    private readonly AppDbContext _db;

    public OperatorsController(AppDbContext db) => _db = db;

    // GET /api/operators
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Operators.ToListAsync());

    // POST /api/operators
    // [FromBody] açıkça yazılmasa da [ApiController] body'den okur.
    [HttpPost]
    public async Task<IActionResult> Create(CreateOperatorRequest request)
    {
        var op = new Operator
        {
            Name = request.Name,
            Phone = request.Phone,
            Location = request.Location
        };

        _db.Operators.Add(op);
        await _db.SaveChangesAsync();

        // CreatedAtAction → 201 + Location header: /api/operators/{id}
        return CreatedAtAction(nameof(GetAll), new { id = op.Id }, op);
    }
}
