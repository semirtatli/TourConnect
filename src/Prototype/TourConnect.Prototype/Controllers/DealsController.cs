using TourConnect.Prototype.Data;
using TourConnect.Prototype.Models;

namespace TourConnect.Prototype.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DealsController : ControllerBase
{
    private readonly AppDbContext _db;

    public DealsController(AppDbContext db) => _db = db;

    // GET /api/deals → sadece Active deal'leri döndür.
    // Expiry kontrolü artık burada değil — DealExpiryService arka planda hallediyor.
    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var activeDeals = await _db.Deals
            .Where(d => d.Status == DealStatus.Active)
            .Include(d => d.Tour)
                .ThenInclude(t => t.Operator)
            .ToListAsync();

        return Ok(activeDeals);
    }

    // POST /api/deals
    [HttpPost]
    public async Task<IActionResult> Create(CreateDealRequest request)
    {
        var tour = await _db.Tours.FindAsync(request.TourId);
        if (tour is null)
            return Problem(detail: $"Tur bulunamadı: {request.TourId}", statusCode: 404);

        // Input validation'lar FluentValidation tarafından yapıldı.
        // Buraya gelindiyse tüm alanlar geçerli demektir.
        var deal = new Deal
        {
            TourId = request.TourId,
            OperatorId = tour.OperatorId,
            AvailableSlots = request.AvailableSlots,
            OriginalPrice = request.OriginalPrice,
            DiscountedPrice = request.DiscountedPrice,
            ExpiresAt = request.ExpiresAt,
            Status = DealStatus.Active
        };

        _db.Deals.Add(deal);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetActive), new { id = deal.Id }, deal);
    }

    // PUT /api/deals/{id}/cancel
    // [HttpPut("{id}/cancel")] → {id} URL'den parametre olarak gelir
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var deal = await _db.Deals.FindAsync(id);
        if (deal is null)
            return Problem(detail: $"Deal bulunamadı: {id}", statusCode: 404);

        if (deal.Status != DealStatus.Active)
            return Problem(detail: $"Bu deal iptal edilemez. Mevcut durum: {deal.Status}", statusCode: 400);

        deal.Status = DealStatus.Cancelled;
        await _db.SaveChangesAsync();

        return Ok(deal);
    }
}
