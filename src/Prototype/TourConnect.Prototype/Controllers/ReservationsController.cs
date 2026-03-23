namespace TourConnect.Prototype.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReservationsController(AppDbContext db) => _db = db;

    // GET /api/reservations
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Reservations
            .Include(r => r.Deal)
                .ThenInclude(d => d.Tour)
            .Include(r => r.Partner)
            .ToListAsync());

    // POST /api/reservations
    [HttpPost]
    public async Task<IActionResult> Create(CreateReservationRequest request)
    {
        var deal = await _db.Deals.FindAsync(request.DealId);
        if (deal is null)
            return NotFound($"Deal bulunamadı: {request.DealId}");

        if (deal.Status != DealStatus.Active)
            return BadRequest($"Bu deal rezervasyon kabul etmiyor. Durum: {deal.Status}");

        if (deal.ExpiresAt <= DateTime.UtcNow)
        {
            deal.Status = DealStatus.Expired;
            await _db.SaveChangesAsync();
            return BadRequest("Deal süresi dolmuş.");
        }

        var partner = await _db.Partners.FindAsync(request.PartnerId);
        if (partner is null)
            return NotFound($"Partner bulunamadı: {request.PartnerId}");

        if (deal.AvailableSlots < request.GuestCount)
            return BadRequest($"Yeterli slot yok. İstenen: {request.GuestCount}, Mevcut: {deal.AvailableSlots}");

        deal.AvailableSlots -= request.GuestCount;

        if (deal.AvailableSlots == 0)
            deal.Status = DealStatus.FullyBooked;

        var reservation = new Reservation
        {
            DealId = request.DealId,
            PartnerId = request.PartnerId,
            GuestName = request.GuestName,
            GuestCount = request.GuestCount,
            Status = ReservationStatus.Confirmed
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = reservation.Id }, reservation);
    }
}
