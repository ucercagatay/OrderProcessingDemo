# Order Processing Demo — Microservices with .NET 8

An event-driven microservices demo showcasing the **Outbox Pattern** with **Apache Kafka** for reliable messaging between services.

## Architecture

```
                    ┌──────────────────┐
  POST /api/orders  │                  │
 ──────────────────>│  Order Service   │
                    │  (ASP.NET Core)  │
                    └───────┬──────────┘
                            │
                     Outbox Pattern
                     (same transaction)
                            │
                    ┌───────┴──────────┐
                    │   PostgreSQL     │
                    │   - Orders       │
                    │   - OutboxMessages│
                    └───────┬──────────┘
                            │
                    BackgroundService
                    (poll & publish)
                            │
                    ┌───────┴──────────┐
                    │      KAFKA       │
                    │  "order-events"  │
                    └───┬─────────┬────┘
                        │         │
              ┌─────────┴──┐  ┌──┴───────────┐
              │ Inventory  │  │ Notification  │
              │ Service    │  │ Service       │
              │            │  │               │
              │ Stock      │  │ Email         │
              │ validation │  │ simulation    │
              └────────────┘  └───────────────┘
```

## Tech Stack

- **Runtime:** .NET 8 / C#
- **API:** ASP.NET Core Web API
- **Architecture:** Clean Architecture, CQRS pattern
- **Database:** PostgreSQL + Entity Framework Core
- **Messaging:** Apache Kafka (KRaft mode, no Zookeeper)
- **Patterns:** Outbox Pattern, Unit of Work, Repository Pattern
- **Containerization:** Docker Compose

## Services

| Service | Description | Port |
|---------|-------------|------|
| Order Service | REST API — creates orders, publishes events via Outbox | 5213 |
| Inventory Service | Kafka consumer — stock validation | Worker |
| Notification Service | Kafka consumer — email simulation | Worker |

## Key Patterns Demonstrated

### Outbox Pattern
Order and outbox message are saved in the **same database transaction**. A background service polls the outbox table and publishes to Kafka. This guarantees **no event loss** even if Kafka is temporarily down.

### Unit of Work
Transaction management is abstracted through `IUnitOfWork` interface, keeping the Application layer independent of infrastructure concerns.

### Clean Architecture
- **Domain** — Entities, enums (zero dependencies)
- **Application** — Use cases, DTOs, interfaces, mappings
- **Infrastructure** — EF Core, Kafka producer, repositories, background services
- **API** — Controllers, DI configuration

## Getting Started

### Prerequisites
- .NET 8 SDK
- Docker Desktop

### Run

```bash
# Start infrastructure (PostgreSQL + Kafka)
docker compose up -d

# Apply database migrations
cd OrderService.Api
dotnet ef database update --project ../OrderService.Infrastructure --startup-project .
cd ..

# Terminal 1: Order Service
dotnet run --project OrderService.Api

# Terminal 2: Inventory Service
dotnet run --project InventoryService

# Terminal 3: Notification Service
dotnet run --project NotificationService
```

### Test

Open Swagger UI: `http://localhost:5213/swagger`

Create an order:
```bash
curl -X POST http://localhost:5213/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":"3fa85f64-5717-4562-b3fc-2c963f66afa6","productId":"3fa85f64-5717-4562-b3fc-2c963f66afa7","quantity":2,"price":29.99}'
```

Expected output across terminals:

**Order Service:**
```
Published outbox message {id} to Kafka
```

**Inventory Service:**
```
📦 INVENTORY CHECK: OrderId=..., ProductId=..., Quantity=2
✅ STOCK RESERVED: OrderId=..., Quantity=2
```

**Notification Service:**
```
📧 NOTIFICATION: New order received!
✅ EMAIL SENT for OrderId=...
```

## Project Structure

```
OrderProcessingDemo/
├── OrderService.Api/                  → HTTP entry point, DI config
├── OrderService.Application/         → Use cases, DTOs, interfaces
├── OrderService.Domain/              → Entities, enums (zero deps)
├── OrderService.Infrastructure/      → EF Core, Kafka, repositories
├── InventoryService/                 → Kafka consumer, stock check
├── NotificationService/              → Kafka consumer, notifications
└── docker-compose.yml                → PostgreSQL + Kafka
```

## Author

**Haktan Çağatay Üçer**
- GitHub: [@ucercagatay](https://github.com/ucercagatay)
- LinkedIn: [ucercagatay](https://linkedin.com/in/ucercagatay)
