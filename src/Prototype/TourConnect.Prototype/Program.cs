using System.Text.Json.Serialization;
using TourConnect.Prototype.Data;

var builder = WebApplication.CreateBuilder(args);

// Controller tabanlı API'yi kaydet.
// AddControllers() → [ApiController] sınıflarını tarar ve endpoint olarak kaydeder.
// Faz 0'da Minimal API kullanıyorduk (app.MapGet/MapPost).
// Şimdi Controller sınıfları otomatik keşfedilir.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Döngüsel referans koruması (Tour → Operator → Tours → Tour → ...)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Swagger için gerekli iki satır.
// AddEndpointsApiExplorer → endpoint meta verilerini toplar
// AddSwaggerGen → bu meta verilerden /swagger UI'ı oluşturur
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core + SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tourconnect.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Migration'ları uygula (uygulama her başladığında)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Controller'ları HTTP pipeline'a bağla.
// Bu satır olmadan controller'lar tanımlı ama erişilemez olur.
app.MapControllers();

app.Run();
