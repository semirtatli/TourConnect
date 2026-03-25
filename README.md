# TourConnect

Last-minute tur eşleştirme platformu. Tur operatörleri dolmayan kontenjanlarını fırsat (deal) olarak girer; partner oteller bu fırsatları görüp misafirlerine sunar ve rezervasyon oluşturur.

---

## Mimari Yolculuk

Bu proje, yazılım mimarisinin evrimini bizzat yaşayarak öğrenmek için tasarlandı. Her faz bir öncekinin üzerine inşa edildi:

| Faz | Açıklama | Teknoloji |
|-----|----------|-----------|
| **0 — Prototip** | Tek dosya, sıfır pattern | Minimal API, SQLite |
| **1 — Monolith** | Controller'lar, validasyon, Docker | PostgreSQL, FluentValidation |
| **2 — Clean Architecture** | Katmanlı mimari, CQRS | MediatR, xUnit |
| **3 — HTTP Mikroservisler** | İki servis, HTTP haberleşmesi | HttpClient, YARP |
| **4 — Event-Driven** | Asenkron iletişim, saga, cache | RabbitMQ, MassTransit, Redis |
| **5 — Prod'a Hazır** | Seed data, health checks, CI/CD | GitHub Actions |

---

## Proje Yapısı

```
TourConnect/
├── src/
│   ├── Prototype/               # Faz 0: SQLite + Minimal API
│   ├── Monolith/                # Faz 1-2: Clean Architecture
│   │   ├── TourConnect.Domain
│   │   ├── TourConnect.Application
│   │   ├── TourConnect.Infrastructure
│   │   ├── TourConnect.API
│   │   └── TourConnect.Application.Tests
│   ├── Services/                # Faz 3-4: Mikroservisler
│   │   ├── TourService/
│   │   └── MatchingService/
│   ├── Gateway/                 # YARP reverse proxy
│   └── BuildingBlocks/          # Paylaşılan event'ler ve tipler
├── docker-compose.yml           # Monolith ortamı
└── docker-compose.microservices.yml  # Mikroservis ortamı
```

---

## Çalıştırma

### Gereksinimler

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- `.env` dosyası (ilk kurulumda oluşturulmalı)

```bash
cp .env.example .env
```

`.env` içine `POSTGRES_DB` ekleyin:

```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=tourconnect

RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
```

---

### Monolith (Faz 1-2)

```bash
docker compose up --build
```

| Servis | URL |
|--------|-----|
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| Health | http://localhost:8080/health |
| PostgreSQL | localhost:5435 |

---

### Mikroservisler (Faz 4)

```bash
docker compose -f docker-compose.microservices.yml up --build
```

| Servis | URL |
|--------|-----|
| Gateway | http://localhost:5000 |
| Tour Service | http://localhost:5001 |
| Matching Service | http://localhost:5002 |
| RabbitMQ Yönetim | http://localhost:15672 (guest/guest) |
| Health — Tour Service | http://localhost:5001/health |
| Health — Matching Service | http://localhost:5002/health |

---

## API

### Monolith Endpoint'leri

#### Operatörler
```
GET  /api/operators          → Tüm operatörleri listele
POST /api/operators          → Yeni operatör oluştur
```
```json
{ "name": "Aegean Blue Tours", "phone": "+90 252 316 4500", "location": "Bodrum" }
```

#### Turlar
```
GET  /api/tours              → Tüm turları listele
POST /api/tours              → Yeni tur oluştur
```
```json
{ "operatorId": "...", "title": "Bodrum Tekne Turu", "description": "...", "category": 0, "durationInHours": 8, "basePrice": 500 }
```
> Kategoriler: `0` BoatTour · `1` Safari · `2` Diving · `3` Cultural · `4` Adventure · `5` Food

#### Deal'lar
```
GET  /api/deals              → Aktif deal'leri listele
POST /api/deals              → Yeni deal oluştur
PUT  /api/deals/{id}/cancel  → Deal'i iptal et
```
```json
{ "tourId": "...", "availableSlots": 8, "originalPrice": 500, "discountedPrice": 350, "expiresAt": "2026-06-30T23:59:59Z" }
```

#### Partner'lar
```
GET  /api/partners           → Tüm partner'ları listele
POST /api/partners           → Yeni partner oluştur
```
```json
{ "name": "Grand Hotel Bodrum", "contactEmail": "info@grandhotel.com", "location": "Bodrum" }
```

#### Rezervasyonlar
```
GET  /api/reservations       → Tüm rezervasyonları listele
POST /api/reservations       → Rezervasyon oluştur
```
```json
{ "dealId": "...", "partnerId": "...", "guestName": "Ali Yılmaz", "guestCount": 3 }
```

### Mikroservis Endpoint'leri (Gateway üzerinden)

Gateway (:5000), gelen isteği path'e göre doğru servise yönlendirir:

| Path | Yönlendirme |
|------|-------------|
| `/api/operators/*`, `/api/tours/*`, `/api/deals/*` | Tour Service (:5001) |
| `/api/partners/*`, `/api/reservations/*` | Matching Service (:5002) |

Mikroservis rezervasyonu **asenkron** çalışır: `POST /api/reservations` → `202 Accepted` döner, arka planda saga tamamlanır.

---

## Rezervasyon Saga'sı (Faz 4)

```
Matching Service          RabbitMQ              Tour Service
     │                       │                       │
     │── ReservationRequested ──────────────────────>│
     │                       │     slot kontrolü     │
     │<─ ReservationConfirmed ──────────────────────│  (yeterli slot)
     │<─ ReservationRejected  ──────────────────────│  (yetersiz slot)
     │
     │  Reservation.Status güncellenir
     │  (Confirmed / Cancelled)
```

---

## Seed Data

Her iki ortam da ilk başlatmada örnek veriyle gelir:

| Veri | İçerik |
|------|--------|
| Operatörler | Aegean Blue Tours (Bodrum), Mediterranean Adventures (Antalya) |
| Turlar | Bodrum Tekne Turu, Bodrum Dalış Macerası, Belek Jeep Safari |
| Deal'lar | 3 aktif deal (8, 4, 12 slot) |
| Partner'lar | Grand Hotel Bodrum, Antalya Palace Hotel |

---

## Testler

```bash
dotnet test TourConnect.sln
```

Monolith Application katmanı için 17 unit test bulunur (MediatR handler'ları ve FluentValidation validator'ları).

---

## CI/CD

Her `push` ve `pull_request` işleminde GitHub Actions otomatik olarak çalışır:

1. `.NET 8` + `.NET 10` kurulumu
2. `dotnet restore`
3. `dotnet build --configuration Release`
4. `dotnet test --configuration Release`

Workflow dosyası: `.github/workflows/ci.yml`

---

## Teknoloji Yığını

| Katman | Teknoloji |
|--------|-----------|
| Backend | .NET 8, ASP.NET Core |
| ORM | Entity Framework Core 8, Npgsql |
| Mesajlaşma | MassTransit, RabbitMQ |
| Cache | Redis, StackExchange.Redis |
| Gateway | YARP |
| Validasyon | FluentValidation |
| CQRS | MediatR |
| Test | xUnit |
| Frontend | React, Vite, Tailwind CSS |
| Veritabanı | PostgreSQL |
| Container | Docker, Docker Compose |
