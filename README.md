# TourConnect

Otel partnerlerinin son dakika tur fırsatlarını rezerve edebildiği bir platform. Tur operatörleri dolmayan kontenjanlarını indirimli deal olarak girer, partner oteller bu deal'leri misafirlerine sunar ve rezervasyon oluşturur.

## Ne yapar?

- Tur operatörleri sisteme kayıt olur, tur ekler, dolmayan turları indirimli deal olarak yayınlar.
- Otel partnerleri aktif deal'leri görür ve misafirleri için rezervasyon oluşturur.
- Rezervasyon oluşturulduğunda `Pending` durumunda kaydedilir. Arka planda Tour Service slot kontrolü yapar ve `Confirmed` ya da `Cancelled` olarak günceller.

## Mimari

```
[React Frontend]
       |
  [YARP Gateway :5000]
       |                   \
[Tour Service :5001]   [Matching Service :5002]
  Operators                 Partners
  Tours                     Reservations
  Deals (Redis cache)
       |                         |
 [PostgreSQL :5436]       [PostgreSQL :5437]
       |                         |
       +----------[RabbitMQ]-----+
```

Gateway hangi isteği nereye yönlendiriyor:

| Path | Servis |
|---|---|
| `/api/operators`, `/api/tours`, `/api/deals` | Tour Service |
| `/api/partners`, `/api/reservations` | Matching Service |

## Rezervasyon nasıl işliyor?

```
Otel partneri  →  POST /api/reservations
                        ↓
             Matching Service rezervasyonu kaydeder  (Status: Pending)
                        ↓  ReservationRequestedEvent yayınlar
             Tour Service slot kontrolü yapar
                 ├── slot var    →  ReservationConfirmedEvent
                 └── slot yok   →  ReservationRejectedEvent
                        ↓
             Matching Service durumu günceller
                 ├── Status: Confirmed
                 └── Status: Cancelled
```

`POST /api/reservations` direkt `202 Accepted` döner. Sonucu `GET /api/reservations/{id}` ile kontrol edebilirsiniz.

## Teknolojiler

| | |
|---|---|
| Backend | .NET 8, ASP.NET Core Minimal API |
| Mimari | Clean Architecture (Domain / Application / Infrastructure) |
| CQRS | MediatR |
| Validasyon | FluentValidation |
| ORM | Entity Framework Core 8 + Npgsql |
| Mesajlaşma | MassTransit + RabbitMQ |
| Cache | Redis (aktif deal'ler, 60 saniyelik TTL) |
| Gateway | YARP |
| Frontend | React + Vite + Tailwind CSS |
| Veritabanı | PostgreSQL 15 (her serviste ayrı DB) |
| Container | Docker + Docker Compose |
| CI/CD | GitHub Actions |
| Test | xUnit |

## Çalıştırma

### Gereksinim

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### 1. `.env` dosyası oluştur

Proje kökünde (`.sln` ile aynı yerde) bir `.env` dosyası oluştur:

```env
POSTGRES_USER=your_db_user
POSTGRES_PASSWORD=your_db_password
POSTGRES_DB=tourconnect
RABBITMQ_USER=your_rabbitmq_user
RABBITMQ_PASSWORD=your_rabbitmq_password
```

Bu dosya `.gitignore`'da, commit edilmez.

### 2. Mikroservisleri başlat

```bash
docker compose -f docker-compose.microservices.yml up --build
```

İlk açılışta migration'lar ve seed data otomatik çalışır. Servisler ayağa kalktıktan sonra:

| | URL |
|---|---|
| Gateway | http://localhost:5000 |
| Tour Service Swagger | http://localhost:5001/swagger |
| Matching Service Swagger | http://localhost:5002/swagger |
| RabbitMQ Yönetim | http://localhost:15672 |

### 3. (İsteğe bağlı) Monolith sürümünü başlat

```bash
docker compose up --build
```

| | URL |
|---|---|
| Swagger | http://localhost:8080/swagger |
| Health | http://localhost:8080/health |

### 4. (İsteğe bağlı) Frontend'i başlat

```bash
cd src/WebApp/tourconnect-frontend
npm install
npm run dev
```

http://localhost:5173 adresinde açılır.

## Endpoint'ler

Aşağıdaki istekler gateway üzerinden (`:5000`) gider. Servislere doğrudan da bağlanabilirsiniz.

**Tour Service**

```
GET  /api/operators      Tüm operatörler
POST /api/operators      Operatör oluştur

GET  /api/tours          Tüm turlar
POST /api/tours          Tur oluştur

GET  /api/deals          Aktif deal'ler (Redis cache, 60s)
POST /api/deals          Deal oluştur
```

**Matching Service**

```
GET  /api/partners           Tüm partnerlar
POST /api/partners           Partner oluştur

GET  /api/reservations       Tüm rezervasyonlar
GET  /api/reservations/{id}  Tek rezervasyon
POST /api/reservations       Rezervasyon oluştur  →  202 Accepted
```

### Hızlı örnek

Aşağıdaki seed ID'leri her ortamda hazır gelir, direkt kullanabilirsin:

```bash
curl -X POST http://localhost:5000/api/reservations \
  -H "Content-Type: application/json" \
  -d '{
    "dealId":    "c1000000-0000-0000-0000-000000000001",
    "partnerId": "d1000000-0000-0000-0000-000000000001",
    "guestName": "Ali Yılmaz",
    "guestCount": 2
  }'
```

## Seed data

İlk başlatmada her iki servise de aşağıdaki veriler eklenir. ID'ler sabit, her ortamda aynı:

| Tip | Ad | ID |
|---|---|---|
| Operator | Aegean Blue Tours | `a1000000-...` |
| Operator | Mediterranean Adventures | `a2000000-...` |
| Tour | Bodrum Tekne Turu | `b1000000-...` |
| Tour | Dalış Macerası | `b2000000-...` |
| Tour | Belek Jeep Safari | `b3000000-...` |
| Deal | Tekne turu — 8 slot | `c1000000-...` |
| Deal | Dalış — 4 slot | `c2000000-...` |
| Deal | Jeep Safari — 12 slot | `c3000000-...` |
| Partner | Grand Hotel Bodrum | `d1000000-...` |
| Partner | Antalya Palace Hotel | `d2000000-...` |

Deal'lerin expiry tarihi `2099-12-31`, yani hiç expire olmaz.

## Health check

```bash
curl http://localhost:5001/health
# {"status":"Healthy","checks":{"database":"Healthy","redis":"Healthy"}}

curl http://localhost:5002/health
# {"status":"Healthy","checks":{"database":"Healthy"}}
```

## Testler

```bash
dotnet test TourConnect.sln
```

Her iki servis için de MediatR handler'larını ve FluentValidation validator'larını kapsayan unit testler var.

## CI/CD

`master`'a her push ve PR'da GitHub Actions çalışır:

1. NuGet paketlerini restore et (`.csproj` hash'e göre cache'li)
2. `dotnet build --configuration Release`
3. `dotnet test --configuration Release`

Workflow: `.github/workflows/ci.yml`

## Proje yapısı

```
src/
├── Prototype/               # tek dosya prototip (SQLite, pattern yok)
├── Monolith/                # Clean Architecture monolith
│   ├── TourConnect.Domain
│   ├── TourConnect.Application
│   ├── TourConnect.Infrastructure
│   ├── TourConnect.API
│   └── TourConnect.Application.Tests
├── Services/
│   ├── TourService/
│   │   ├── TourConnect.TourService.Domain
│   │   ├── TourConnect.TourService.Application
│   │   ├── TourConnect.TourService.Infrastructure
│   │   ├── TourConnect.TourService          ← giriş noktası
│   │   └── TourConnect.TourService.Tests
│   └── MatchingService/
│       ├── TourConnect.MatchingService.Domain
│       ├── TourConnect.MatchingService.Application
│       ├── TourConnect.MatchingService.Infrastructure
│       ├── TourConnect.MatchingService      ← giriş noktası
│       └── TourConnect.MatchingService.Tests
├── Gateway/
│   └── TourConnect.Gateway                  ← YARP
└── BuildingBlocks/
    ├── Common/              ← BaseEntity, Result<T>
    └── EventBus.Messages/   ← servisler arası paylaşılan event'ler
```

Her servis Clean Architecture'a göre yapılandırılmış. `Application` katmanının framework bağımlılığı yok — sadece interface'ler, MediatR handler'ları ve FluentValidation validator'ları var.
