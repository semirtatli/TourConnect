namespace TourConnect.Prototype.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartnersController : ControllerBase
{
    private readonly AppDbContext _db;

    public PartnersController(AppDbContext db) => _db = db;

    // GET /api/partners
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Partners.ToListAsync());

    // POST /api/partners
    [HttpPost]
    public async Task<IActionResult> Create(CreatePartnerRequest request)
    {
        var partner = new Partner
        {
            Name = request.Name,
            ContactEmail = request.ContactEmail,
            Location = request.Location
        };

        _db.Partners.Add(partner);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = partner.Id }, partner);
    }
}
