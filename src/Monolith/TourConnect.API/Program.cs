using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TourConnect.Application;
using TourConnect.Application.Operators;
using TourConnect.Infrastructure.BackgroundServices;
using TourConnect.Infrastructure.Persistence;
using TourConnect.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// --- CONTROLLER ---
// AddControllers: [ApiController] sınıflarını bulur ve endpoint olarak kaydeder.
// ReferenceHandler.IgnoreCycles: döngüsel referansları yok say
// (Tour → Operator → Tours → Tour → ... sonsuz döngüsünü önler)
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// --- SWAGGER ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- FLUENTVALIDATION ---
// AddFluentValidationAutoValidation: validation başarısız olursa
// controller metodu çalışmaz, otomatik 400 döner.
// AddValidatorsFromAssemblyContaining: Application projesindeki tüm validator'ları tarar.
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOperatorValidator>();

// --- MEDIATR ---
// MediatR'ı kaydet. Application projesindeki tüm handler'ları otomatik bulur.
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOperatorCommand).Assembly));

// --- VERITABANI ---
// IAppDbContext → AppDbContext: handler'lar IAppDbContext ister, DI AppDbContext verir.
// AddDbContext: her HTTP isteği için yeni bir DbContext oluşturur (Scoped lifetime).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// --- BACKGROUND SERVICE ---
builder.Services.AddHostedService<DealExpiryService>();

// --- CORS ---
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()));

var app = builder.Build();

// --- SWAGGER UI ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- MIDDLEWARE PIPELINE ---
// Sıra önemli: exception middleware en başta olmalı ki
// sonraki tüm katmanların hatalarını yakalayabilsin.
app.UseCors();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// --- MİGRASYON ---
// Uygulama her başladığında bekleyen migration'ları uygula.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapControllers();
app.Run();
