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
}

// AppDbContext: EF Core'un veritabanıyla konuştuğu köprü sınıf.
// DbSet<Operator> → "operators" tablosunu temsil eder.
// Her DbSet bir tablodur.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Operator> Operators => Set<Operator>();
}

// Request DTO: POST endpoint'ine gelen JSON'un şekli.
// "DTO" = Data Transfer Object = sadece veri taşıyan nesne.
// Neden ayrı bir sınıf? Çünkü kullanıcı Id veya CreatedAt göndermemeli,
// sadece ihtiyacı olan alanları görmeli.
public record CreateOperatorRequest(string Name, string Phone, string Location);
