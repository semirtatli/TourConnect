var builder = WebApplication.CreateBuilder(args);

// YARP: Yet Another Reverse Proxy
// Gelen isteğin path'ine bakarak hangi servise yönlendireceğine karar verir.
// Client sadece Gateway'i bilir (:5000), servis adreslerini bilmez.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Teşhis endpoint'i: gateway'in çalıştığını doğrular
app.MapGet("/health", () => "gateway ok");

app.MapReverseProxy();
app.Run();
