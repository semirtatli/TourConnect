# =====================================================================
# STAGE 1: BUILD
# mcr.microsoft.com/dotnet/sdk:8.0 → .NET SDK image'ı
# SDK = derleyici + araçlar + runtime. Büyük (~800MB) ama derleme için gerekli.
# "AS build" → bu stage'e isim veriyoruz, aşağıda referans alacağız.
# =====================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Önce sadece .csproj dosyasını kopyala ve restore et.
# Neden? Docker her satırı "layer" olarak cache'ler.
# Sadece NuGet paketleri değiştiyse bu layer yeniden çalışır.
# Kod değiştiyse ama paket değişmediyse bu adım cache'den gelir → hızlı build.
COPY ["src/Prototype/TourConnect.Prototype/TourConnect.Prototype.csproj", "src/Prototype/TourConnect.Prototype/"]
RUN dotnet restore "src/Prototype/TourConnect.Prototype/TourConnect.Prototype.csproj"

# Tüm kaynak kodu kopyala ve Release modunda derle.
# Release modu: optimizasyonlar açık, debug sembolleri kapalı → daha hızlı, daha küçük.
COPY . .
WORKDIR "/src/src/Prototype/TourConnect.Prototype"
RUN dotnet publish -c Release -o /app/publish --no-restore

# =====================================================================
# STAGE 2: RUNTIME
# mcr.microsoft.com/dotnet/aspnet:8.0 → sadece runtime image'ı
# SDK yok, derleyici yok. Sadece .NET uygulaması çalıştırmak için gereken parçalar.
# Çok daha küçük (~200MB) → production için ideal.
# =====================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

# Build stage'inden derlenen dosyaları kopyala.
# "--from=build" → stage 1'den al.
COPY --from=build /app/publish .

# Container başladığında çalışacak komut.
ENTRYPOINT ["dotnet", "TourConnect.Prototype.dll"]
