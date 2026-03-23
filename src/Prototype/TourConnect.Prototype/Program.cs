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

// AppDbContext: EF Core'un veritabanıyla konuştuğu köprü sınıf.
// Her DbSet<T> bir DB tablosuna karşılık gelir.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<Tour> Tours => Set<Tour>(); // ← yeni eklendi
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
