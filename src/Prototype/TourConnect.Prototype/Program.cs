using Microsoft.EntityFrameworkCore;

// =====================================================================
// UYGULAMA KURULUM BLOĞU
// WebApplication.CreateBuilder() → uygulamayı yapılandırmak için bir
// "builder" nesnesi oluşturur. Servisler buraya kayıt edilir.
// =====================================================================
var builder = WebApplication.CreateBuilder(args);

// Swagger: API endpoint'lerini otomatik belgeleyen bir UI.
// Tarayıcıda /swagger açarak tüm endpoint'leri görebilir, test edebilirsin.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JSON serileştirici ayarları.
// ReferenceHandler.IgnoreCycles → A→B→A gibi döngüsel referanslarda
// ikinci kez karşılaşılan nesneyi null yazar, sonsuz döngüyü önler.
// Örnek: Tour.Operator.Tours → Tours listesi null gelir (zaten Tour'dan bakıyoruz).
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// EF Core + SQLite kaydı.
// "AppDbContext" diye bir sınıf yazacağız (aşağıda), bu satır onu
// dependency injection sistemine tanıtır.
// "tourconnect.db" → SQLite veritabanı dosyasının adı.
// Uygulama ilk çalışınca bu dosya otomatik oluşur.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tourconnect.db"));

// =====================================================================
// UYGULAMA ÇALIŞTIRMA BLOĞU
// builder.Build() → artık yapılandırma bitti, uygulamayı oluştur.
// Bundan sonra middleware ve endpoint tanımları gelir.
// =====================================================================
var app = builder.Build();

// Geliştirme ortamında Swagger UI'ı aç.
// ASPNETCORE_ENVIRONMENT=Development olduğunda çalışır.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =====================================================================
// VERİTABANI BAŞLATMA
// Uygulama her başladığında migration'ları uygula.
// Migration = "DB şeması nasıl olmalı" talimatı.
// Bu satır olmasaydı tabloları elle oluşturmamız gerekirdi.
// =====================================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// =====================================================================
// ENDPOINT'LER
// MapGet / MapPost → HTTP metoduna göre endpoint tanımı.
// "/api/operators" → URL path.
// async (AppDbContext db) → EF Core context'ini otomatik inject et.
// =====================================================================

// GET /api/operators → Tüm operatörleri listele
app.MapGet("/api/operators", async (AppDbContext db) =>
    await db.Operators.ToListAsync());

// POST /api/operators → Yeni operatör oluştur
// "CreateOperatorRequest request" → HTTP body'den JSON olarak gelir
app.MapPost("/api/operators", async (CreateOperatorRequest request, AppDbContext db) =>
{
    // Gelen veriden yeni bir Operator nesnesi oluştur
    var op = new Operator
    {
        Name = request.Name,
        Phone = request.Phone,
        Location = request.Location
    };

    db.Operators.Add(op);        // EF Core'a "bunu ekleyeceksin" de
    await db.SaveChangesAsync(); // DB'ye yaz

    // 201 Created → başarıyla oluşturuldu, yeni nesneyi döndür
    return Results.Created($"/api/operators/{op.Id}", op);
});

// GET /api/tours → Tüm turları listele
// Include(t => t.Operator) → ilişkili operatör verisini de çek (JOIN yapar).
// Normalde t.Operator null gelir, Include olmadan sadece OperatorId görünür.
app.MapGet("/api/tours", async (AppDbContext db) =>
    await db.Tours.Include(t => t.Operator).ToListAsync());

// POST /api/tours → Yeni tur oluştur
app.MapPost("/api/tours", async (CreateTourRequest request, AppDbContext db) =>
{
    // Önce operatörün gerçekten var olup olmadığını kontrol et.
    // FindAsync → primary key'e göre arar, en hızlı yöntem.
    var operatorExists = await db.Operators.FindAsync(request.OperatorId);
    if (operatorExists is null)
        // 404 Not Found → "Bu ID'de operatör yok"
        return Results.NotFound($"Operatör bulunamadı: {request.OperatorId}");

    var tour = new Tour
    {
        OperatorId = request.OperatorId,
        Title = request.Title,
        Description = request.Description,
        Category = request.Category,   // enum değeri doğrudan atanır
        DurationInHours = request.DurationInHours,
        BasePrice = request.BasePrice
    };

    db.Tours.Add(tour);
    await db.SaveChangesAsync();

    return Results.Created($"/api/tours/{tour.Id}", tour);
});

// GET /api/deals → Sadece aktif deal'leri listele
// Burada iki iş yapıyoruz:
//   1. Süresi geçmiş deal'leri Expired yap (expiry kontrolü)
//   2. Hâlâ Active olanları döndür
// Not: Bu "request anında kontrol" yöntemi. Faz 1'de bunu background service'e taşıyacağız.
app.MapGet("/api/deals", async (AppDbContext db) =>
{
    // Süresi geçmiş ama hâlâ Active görünen deal'leri bul
    // DateTime.UtcNow → sunucunun şu anki zamanı (UTC, timezone farkı olmadan)
    // Where() → SQL WHERE koşulu
    var expiredDeals = await db.Deals
        .Where(d => d.ExpiresAt < DateTime.UtcNow && d.Status == DealStatus.Active)
        .ToListAsync();

    // Bulunanları Expired yap ve kaydet
    foreach (var deal in expiredDeals)
        deal.Status = DealStatus.Expired;

    // Değişiklik varsa DB'ye yaz (yoksa boşuna sorgu atmaz)
    if (expiredDeals.Count > 0)
        await db.SaveChangesAsync();

    // Artık sadece Active deal'leri getir, Tour bilgisiyle birlikte
    return await db.Deals
        .Where(d => d.Status == DealStatus.Active)
        .Include(d => d.Tour)           // JOIN tours
            .ThenInclude(t => t.Operator) // JOIN operators (tour üzerinden)
        .ToListAsync();
});

// POST /api/deals → Yeni last-minute fırsat oluştur
app.MapPost("/api/deals", async (CreateDealRequest request, AppDbContext db) =>
{
    // Turun var olduğunu kontrol et
    var tour = await db.Tours.FindAsync(request.TourId);
    if (tour is null)
        return Results.NotFound($"Tur bulunamadı: {request.TourId}");

    // İş kuralı: indirimli fiyat orijinal fiyattan düşük olmalı
    if (request.DiscountedPrice >= request.OriginalPrice)
        return Results.BadRequest("İndirimli fiyat orijinal fiyattan düşük olmalı.");

    // İş kuralı: bitiş zamanı gelecekte olmalı
    if (request.ExpiresAt <= DateTime.UtcNow)
        return Results.BadRequest("Bitiş zamanı gelecekte olmalı.");

    // İş kuralı: en az 1 slot olmalı
    if (request.AvailableSlots <= 0)
        return Results.BadRequest("En az 1 slot olmalı.");

    var deal = new Deal
    {
        TourId = request.TourId,
        OperatorId = tour.OperatorId,       // turu kim veriyorsa o operatör
        AvailableSlots = request.AvailableSlots,
        OriginalPrice = request.OriginalPrice,
        DiscountedPrice = request.DiscountedPrice,
        ExpiresAt = request.ExpiresAt,
        Status = DealStatus.Active          // yeni deal her zaman Active başlar
    };

    db.Deals.Add(deal);
    await db.SaveChangesAsync();

    return Results.Created($"/api/deals/{deal.Id}", deal);
});

// PUT /api/deals/{id}/cancel → Operatör fırsatı iptal eder
// PUT = "bu kaydı güncelle" anlamındaki HTTP metodu
// {id} → URL'den gelen parametre: /api/deals/abc-123/cancel
app.MapPut("/api/deals/{id}/cancel", async (Guid id, AppDbContext db) =>
{
    var deal = await db.Deals.FindAsync(id);

    if (deal is null)
        return Results.NotFound($"Deal bulunamadı: {id}");

    // Sadece Active deal iptal edilebilir
    if (deal.Status != DealStatus.Active)
        return Results.BadRequest($"Bu deal iptal edilemez. Mevcut durum: {deal.Status}");

    deal.Status = DealStatus.Cancelled;
    await db.SaveChangesAsync();

    return Results.Ok(deal);
});

// GET /api/partners → Tüm partner otelleri listele
app.MapGet("/api/partners", async (AppDbContext db) =>
    await db.Partners.ToListAsync());

// POST /api/partners → Yeni partner otel kaydı
app.MapPost("/api/partners", async (CreatePartnerRequest request, AppDbContext db) =>
{
    var partner = new Partner
    {
        Name = request.Name,
        ContactEmail = request.ContactEmail,
        Location = request.Location
    };

    db.Partners.Add(partner);
    await db.SaveChangesAsync();

    return Results.Created($"/api/partners/{partner.Id}", partner);
});

// GET /api/reservations → Tüm rezervasyonları listele (deal ve partner bilgisiyle)
app.MapGet("/api/reservations", async (AppDbContext db) =>
    await db.Reservations
        .Include(r => r.Deal)       // JOIN deals
            .ThenInclude(d => d.Tour)   // JOIN tours (deal üzerinden)
        .Include(r => r.Partner)    // JOIN partners
        .ToListAsync());

// POST /api/reservations → Yeni rezervasyon oluştur
// Bu endpoint projenin kalbindeki iş mantığını içeriyor:
// slot kontrolü + slot düşürme + status yönetimi
app.MapPost("/api/reservations", async (CreateReservationRequest request, AppDbContext db) =>
{
    // Deal'i bul. Active mi? Süresi geçmemiş mi?
    var deal = await db.Deals.FindAsync(request.DealId);
    if (deal is null)
        return Results.NotFound($"Deal bulunamadı: {request.DealId}");

    // Sadece Active deal'e rezervasyon yapılabilir.
    // Expired, FullyBooked veya Cancelled deal'ler için işlem yapmıyoruz.
    if (deal.Status != DealStatus.Active)
        return Results.BadRequest($"Bu deal rezervasyon kabul etmiyor. Durum: {deal.Status}");

    // GET /api/deals her istekte expired kontrolü yapıyor ama rezervasyon
    // endpoint'i doğrudan çağrılabilir. Burada anlık kontrol ekliyoruz:
    // süresi geçmişse önce DB'de Expired yap, sonra reddet.
    if (deal.ExpiresAt <= DateTime.UtcNow)
    {
        deal.Status = DealStatus.Expired;
        await db.SaveChangesAsync();
        return Results.BadRequest("Deal süresi dolmuş.");
    }

    // Partner'ın var olduğunu kontrol et
    var partner = await db.Partners.FindAsync(request.PartnerId);
    if (partner is null)
        return Results.NotFound($"Partner bulunamadı: {request.PartnerId}");

    // Kritik iş kuralı: yeterli slot var mı?
    // GuestCount = misafir sayısı, AvailableSlots = kalan yer
    if (deal.AvailableSlots < request.GuestCount)
        return Results.BadRequest(
            $"Yeterli slot yok. İstenen: {request.GuestCount}, Mevcut: {deal.AvailableSlots}");

    // Slot düş: rezervasyon onaylandığı anda yer bloke edilir
    deal.AvailableSlots -= request.GuestCount;

    // Eğer kalan slot 0'a düştüyse deal'i FullyBooked yap.
    // Artık yeni rezervasyon kabul edilmeyecek.
    if (deal.AvailableSlots == 0)
        deal.Status = DealStatus.FullyBooked;

    var reservation = new Reservation
    {
        DealId = request.DealId,
        PartnerId = request.PartnerId,
        GuestName = request.GuestName,
        GuestCount = request.GuestCount,
        // Faz 0'da "anında onayla" → Confirmed.
        // Faz 4'te bu Pending olacak, saga tamamlanınca Confirmed geçecek.
        Status = ReservationStatus.Confirmed
    };

    db.Reservations.Add(reservation);
    await db.SaveChangesAsync(); // Hem deal güncelleme hem rezervasyon tek transaction'da yazılır

    return Results.Created($"/api/reservations/{reservation.Id}", reservation);
});

app.Run(); // Uygulamayı başlat, HTTP isteklerini dinlemeye başla

// =====================================================================
// VERİ MODELLERİ
// Bu prototipte tüm modelleri aynı dosyada tutuyoruz.
// Faz 1'de bunları ayrı dosyalara taşıyacağız.
// =====================================================================

// Operator: Tur düzenleyen şirket veya kişi.
// Id → her kayıt için benzersiz kimlik (Guid = globally unique identifier).
// IsActive → yumuşak silme: kaydı silmek yerine pasif yaparız.
public class Operator
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property: EF Core bu operatöre ait turları buraya doldurur.
    // "virtual" → lazy loading için (şimdilik kullanmıyoruz ama iyi pratik).
    public List<Tour> Tours { get; set; } = [];
}

// TourCategory: Turun türünü belirten enum.
// DB'de 0,1,2... olarak saklanır. Kodda BoatTour, Safari... diye okunur.
public enum TourCategory
{
    BoatTour,    // 0
    Safari,      // 1
    Diving,      // 2
    Cultural,    // 3
    Adventure,   // 4
    Food         // 5
}

// Tour: Bir operatörün sunduğu tur.
// OperatorId → hangi operatöre ait olduğunu söyler (foreign key).
// Operator → EF Core bu property'yi otomatik doldurur (navigation property).
public class Tour
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OperatorId { get; set; }              // foreign key
    public Operator Operator { get; set; } = null!;   // navigation property
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TourCategory Category { get; set; }
    public int DurationInHours { get; set; }
    public decimal BasePrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// DealStatus: Deal'in anlık durumu.
// Enum kullanıyoruz çünkü bu 4 değerin dışında bir şey olamaz.
public enum DealStatus
{
    Active,       // 0 → yayında, rezervasyon yapılabilir
    Expired,      // 1 → süresi doldu, otomatik geçiş
    FullyBooked,  // 2 → tüm slotlar doldu
    Cancelled     // 3 → operatör iptal etti
}

// Deal: Bir tura ait last-minute fırsat.
// Tour ile ilişkili ama bağımsız bir yaşam döngüsü var.
// Aynı tura birden fazla deal açılabilir (farklı günler için).
public class Deal
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Hangi tura ait? (foreign key)
    public Guid TourId { get; set; }
    public Tour Tour { get; set; } = null!;  // navigation property

    // Turu kim veriyor? (kolaylık için saklıyoruz — Tour.OperatorId ile aynı)
    public Guid OperatorId { get; set; }

    // Kaç kişilik yer var? Rezervasyon yapıldıkça azalır.
    public int AvailableSlots { get; set; }

    // decimal → para için doğru tip (float/double yuvarlama hatası yapar)
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }

    // Bu zamandan sonra deal geçersiz sayılır
    public DateTime ExpiresAt { get; set; }

    // Şu anki durum. Her zaman Active olarak başlar.
    public DealStatus Status { get; set; } = DealStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Partner: Rezervasyon yapan otel veya acente.
// ContactEmail → partner iletişim adresi, bildirimler buraya gider (Faz 1'de).
public class Partner
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Bu partner'ın tüm rezervasyonları (navigation property)
    public List<Reservation> Reservations { get; set; } = [];
}

// ReservationStatus: Rezervasyonun anlık durumu.
// Faz 0: Confirmed veya Cancelled (saga yok, anında onay).
// Faz 4: Pending eklenecek (saga tamamlanana kadar bekler).
public enum ReservationStatus
{
    Confirmed,  // 0 → slot düşüldü, onaylandı
    Cancelled   // 1 → iptal edildi (Faz 0'da endpoint yok, ileride eklenecek)
}

// Reservation: Partner'ın bir deal'e yaptığı rezervasyon.
// Deal.AvailableSlots bu kayıt oluşunca azalır.
public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Hangi fırsata rezervasyon? (foreign key)
    public Guid DealId { get; set; }
    public Deal Deal { get; set; } = null!;

    // Hangi partner yaptı? (foreign key)
    public Guid PartnerId { get; set; }
    public Partner Partner { get; set; } = null!;

    public string GuestName { get; set; } = string.Empty; // misafir adı
    public int GuestCount { get; set; }                    // kaç kişilik

    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// AppDbContext: EF Core'un veritabanıyla konuştuğu köprü sınıf.
// Her DbSet<T> bir DB tablosuna karşılık gelir.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Partner> Partners => Set<Partner>();           // ← yeni
    public DbSet<Reservation> Reservations => Set<Reservation>(); // ← yeni
}

// Request DTO'lar: POST endpoint'lerine gelen JSON'un şekli.
// "record" → immutable (değiştirilemez), equality otomatik, concise syntax.
public record CreateOperatorRequest(string Name, string Phone, string Location);

public record CreateTourRequest(
    Guid OperatorId,        // hangi operatöre ait
    string Title,
    string Description,
    TourCategory Category,  // JSON'da 0,1,2 veya "BoatTour","Safari" gönderilebilir
    int DurationInHours,
    decimal BasePrice
);

public record CreateDealRequest(
    Guid TourId,            // hangi tura ait
    int AvailableSlots,     // kaç kişilik yer var
    decimal OriginalPrice,  // normal fiyat
    decimal DiscountedPrice,// indirimli fiyat (< OriginalPrice olmalı)
    DateTime ExpiresAt      // ne zaman sona erecek (gelecekte olmalı)
);

public record CreatePartnerRequest(
    string Name,
    string ContactEmail,
    string Location
);

public record CreateReservationRequest(
    Guid DealId,        // hangi fırsata rezervasyon
    Guid PartnerId,     // hangi partner yapıyor
    string GuestName,   // misafir adı
    int GuestCount      // kaç kişilik (deal.AvailableSlots'tan düşülür)
);
