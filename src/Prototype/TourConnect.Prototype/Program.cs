using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using TourConnect.Prototype.Data;
using TourConnect.Prototype.Middleware;
using TourConnect.Prototype.Validators;

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

// FluentValidation: tüm validator sınıflarını otomatik bul ve kaydet.
// AddValidatorsFromAssemblyContaining → belirtilen sınıfın bulunduğu assembly'deki
// tüm AbstractValidator<T> sınıflarını tarar.
// AddFluentValidationAutoValidation → [ApiController] ile entegre olur:
// validation başarısız olunca controller method'u çalışmaz, otomatik 400 döner.
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateDealValidator>();

// EF Core + PostgreSQL
// GetConnectionString("DefaultConnection") → appsettings.json'daki
// ConnectionStrings.DefaultConnection değerini okur.
// Prod'da bu değer environment variable ile ezilir (güvenli).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Exception middleware'i pipeline'ın en başına ekle.
// En başta olması gerekiyor: sonraki tüm middleware ve controller'ların
// fırlattığı exception'ları yakalamak için.
app.UseMiddleware<ExceptionHandlingMiddleware>();

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
