# 🏬 ASA Tenant — Backend Service (ASA-TENANT-BE)

> The **Tenant Service** of the ASA microservice system — powers all shop-level operations including POS, inventory, orders, customers, payments, NFC, real-time notifications, and AI chatbot.

---

## 🏗️ System Architecture

ASA is a **multi-tenant SaaS platform** for retail shop management, built as two independent microservices that communicate via HTTP:

```
┌─────────────────────────────────────────────────────────┐
│                     ASA SYSTEM                          │
│                                                         │
│  ┌──────────────────┐        ┌──────────────────────┐  │
│  │  ASA-PLATFORM-BE │◄──────►│   ASA-TENANT-BE      │  │
│  │  (Port 8081)     │  HTTP  │   (Port 8080)        │  │
│  │                  │        │                      │  │
│  │  - Platform Admin│        │  - Shop Operations   │  │
│  │  - Shop Mgmt     │        │  - POS / Orders      │  │
│  │  - Subscriptions │        │  - Inventory         │  │
│  │  - Platform Rpts │        │  - Customers / NFC   │  │
│  │  - Feature Flags │        │  - Payments          │  │
│  └──────────────────┘        └──────────────────────┘  │
│           │                            │                │
│    PostgreSQL (Platform DB)    PostgreSQL (Tenant DB)   │
└─────────────────────────────────────────────────────────┘
```

---

## 📖 About This Service

**ASA-TENANT-BE** is the shop-level backend responsible for all day-to-day operations of each tenant (shop) on the ASA platform:
- Point-of-sale order management with multi-payment support
- Product catalog with categories, units, and inventory tracking
- Customer loyalty system with rank tiers
- NFC-based customer identification
- Real-time notifications via SignalR
- AI-powered chatbot (Google Gemini with fallback)
- Voucher and promotion management
- Staff shift management
- Automated weekly/monthly revenue reports
- Subscription management with expiry reminders

---

## ✨ Features & API Modules

### Authentication (`/api/Authentication`)
- Shop owner / staff login with JWT
- Role-based access (Owner, Staff, etc.)
- Custom 401/403 JSON responses

### Shop Management (`/api/Shop`)
- Shop profile CRUD
- Shop subscription status
- Monthly AI request quota reset

### User Management (`/api/User`)
- Staff account management
- Profile with Cloudinary image upload
- Feature-based access control per user (`/api/UserFeature`)

### Product Management (`/api/Product`)
- Full product CRUD with image upload
- Category management (`/api/Category`)
- Unit and product unit management (`/api/Unit`, `/api/ProductUnit`)

### Order Management (`/api/Order`, `/api/OrderDetail`)
- Create and manage POS orders
- Multi-item order details
- Order status flow (Pending → Confirmed → Completed / Cancelled)
- Auto-cancellation of expired unpaid orders (every 1 minute via cron)

### Payment (`/api/Transaction`, `/api/SepayWebhook`)
- SePay payment gateway integration
- ZaloPay payment support
- Webhook handler for payment confirmation
- Multi-tenant payment routing

### Customer Management (`/api/Customer`)
- Customer profile and purchase history
- Loyalty rank system (`/api/Rank`) — tier-based rewards
- NFC card linking (`/api/Nfc`) — identify customers by NFC tap

### Promotion & Voucher (`/api/Promotion`, `/api/PromotionProduct`, `/api/Voucher`)
- Time-limited promotions with product mapping
- Voucher creation and redemption
- Daily promotion expiry automation

### Inventory (`/api/InventoryTransaction`)
- Stock in/out transaction tracking
- Inventory history per product

### Shift Management (`/api/Shift`)
- Staff shift scheduling and tracking
- Open/close shift with cash reconciliation

### Reporting (`/api/Report`)
- Revenue reports by day, week, month
- Automated weekly report (every Monday 00:05 VN time)
- Automated monthly report (1st of month 00:30 VN time)
- Excel export via ClosedXML

### Notifications (`/api/Notification`)
- In-app notification management
- Firebase Cloud Messaging (FCM) push notifications (`/api/Fcm`)
- Real-time delivery via SignalR WebSocket hub (`/notificationHub`)

### AI Chatbot (`/api/Chatbot`, `/api/Prompt`)
- Google Gemini AI chatbot with configurable prompts
- Fallback to hardcoded responses
- Response caching (5-minute TTL)
- Chat message history (`/api/ChatMessage`)

### Subscription (`/api/ShopSubscription`)
- Manage shop subscription plans
- Expiry reminder notifications (every 1 minute check)
- Monthly request quota reset

### Activity Logs (`/api/LogActivity`)
- Full audit trail of all shop operations

---

## ⚙️ Scheduled Jobs (Quartz.NET)

| Job | Schedule | Description |
|---|---|---|
| `WeeklyReportJob` | Every Monday 00:05 (VN time) | Auto-generate weekly revenue reports |
| `MonthlyReportJob` | 1st of month 00:30 (VN time) | Auto-generate monthly revenue reports |
| `OrderExpirationJob` | Every 1 minute | Cancel unpaid orders past expiry time |
| `SubscriptionExpiryReminderJob` | Every 1 minute | Send reminders for expiring subscriptions |
| `PromotionExpiryJob` | Daily 00:15 (VN time) | Expire outdated promotions and vouchers |
| `ShopRequestResetJob` | 1st of month 00:00 (VN time) | Reset monthly AI request quota per shop |

---

## 🛠️ Tech Stack

| Technology | Details |
|---|---|
| Framework | ASP.NET Core 8.0 Web API |
| Language | C# (.NET 8) |
| Database | PostgreSQL (Aiven Cloud) |
| ORM | Entity Framework Core (Npgsql) |
| Authentication | JWT Bearer (HS256) |
| Real-time | SignalR WebSocket Hub |
| Object Mapping | AutoMapper 15 |
| Scheduler | Quartz.NET |
| Image Storage | Cloudinary |
| Email | Brevo (SMTP via HttpClient) |
| Push Notifications | Firebase Cloud Messaging (FCM) |
| AI | Google Gemini API (gemini-2.0-flash) |
| Payment | SePay, ZaloPay |
| Password Hashing | BCrypt.Net-Next |
| Excel Export | ClosedXML |
| API Docs | Swagger / Swashbuckle |
| Containerization | Docker |
| Deployment | Render (port 8080) |

---

## 🏛️ Project Structure

```
ASA-TENANT-BE/
├── ASA-TENANT-BE/            # API Layer
│   ├── Controllers/          # REST API endpoints (28 controllers)
│   ├── Hubs/                 # SignalR NotificationHub
│   ├── Custom Attribute/     # Custom authorization attributes
│   ├── Implement/            # API-layer implementations
│   ├── Templates/            # Email HTML templates
│   └── Program.cs            # DI registration, middleware pipeline
├── ASA-TENANT-SERVICE/       # Business Logic Layer
│   ├── Interface/            # Service interfaces
│   ├── Implenment/           # Service implementations
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Mapping/              # AutoMapper profiles
│   ├── CronJobs/             # Quartz scheduled jobs
│   ├── Configurations/       # Cloudinary, Firebase, Gemini configs
│   ├── Helper/               # Utility helpers
│   ├── Extensions/           # Service collection extensions
│   └── Enums/                # Shared enumerations
├── ASA-TENANT-REPO/          # Data Access Layer
│   ├── Models/               # EF Core entity models (27 entities)
│   ├── DBContext/            # ASATENANTDBContext
│   ├── Repository/           # Repository classes
│   └── Basic/                # Generic base repository
└── ASA-TENANT-REPO-TEST/     # Unit tests for repository layer
```

### Domain Entities

| Entity | Description |
|---|---|
| Shop | Tenant shop profile |
| User | Shop staff and owners |
| Product | Product catalog |
| Category | Product categories |
| Unit / ProductUnit | Units of measurement |
| Order / OrderDetail | POS orders and line items |
| Customer | Customer profiles |
| Rank | Customer loyalty tiers |
| Nfc | NFC card to customer mapping |
| Promotion / PromotionProduct | Discount promotions |
| Voucher | Redeemable vouchers |
| Transaction / Zalopay | Payment records |
| InventoryTransaction | Stock movement history |
| Shift | Staff shift records |
| Report / ReportDetail | Revenue reports |
| Notification / Fcm | Push notification records |
| ChatMessage / Prompt | AI chatbot history |
| ShopSubscription | Subscription plan records |
| UserFeature | Feature access per user |
| LogActivity | Audit logs |

---

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL database
- Docker (optional)

### Configuration

Create or update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Port=...;Database=ASA-TENANT-DB;Username=...;Password=..."
  },
  "JwtConfig": {
    "Issuer": "AsaPlatformBe",
    "Audience": "AsaPlatformUser",
    "Key": "<your-jwt-secret>",
    "ExpireMinutes": 180
  },
  "CloudinarySettings": {
    "CloudName": "<cloud-name>",
    "ApiKey": "<api-key>",
    "ApiSecret": "<api-secret>"
  },
  "GeminiSettings": {
    "ApiKey": "<gemini-api-key>",
    "Model": "gemini-2.0-flash-latest",
    "MaxTokens": 2048,
    "Temperature": 0.7
  },
  "ChatbotSettings": {
    "EnableGeminiAI": true,
    "FallbackToHardcoded": true,
    "CacheResponses": true,
    "CacheExpiryMinutes": 5
  },
  "Firebase": {
    "ProjectId": "<firebase-project-id>",
    "ServiceAccountKey": "{ ... }"
  },
  "BEPlatformUrl": {
    "Url": "https://<asa-platform-be-url>"
  }
}
```

### Run Locally

```bash
cd ASA-TENANT-BE
dotnet restore ASA-TENANT-BE.sln
dotnet run --project ASA-TENANT-BE/ASA-TENANT-BE.csproj
```

API runs on `http://localhost:8080`
Swagger UI: `http://localhost:8080/swagger`
SignalR Hub: `http://localhost:8080/notificationHub`

### Run with Docker

```bash
cd ASA-TENANT-BE
docker build -t asa-tenant-be .
docker run -p 8080:8080 asa-tenant-be
```

---

## 🔗 Service Communication

This service communicates with **ASA-PLATFORM-BE** via HTTP client:

```csharp
// Configured in Program.cs
builder.Services.AddHttpClient("BePlatform", client =>
{
    client.BaseAddress = new Uri(configuration["BEPlatformUrl:Url"]);
    client.Timeout = TimeSpan.FromSeconds(60);
});
```

---

## 📡 Real-time (SignalR)

Clients connect to `/notificationHub` for real-time push notifications:

```javascript
// Frontend connection example
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://<tenant-be-url>/notificationHub", {
    accessTokenFactory: () => token
  })
  .build();

connection.on("ReceiveNotification", (message) => {
  console.log(message);
});

await connection.start();
```

---

## 📚 Course Information

- **Course**: EXE201 — Entrepreneurship Execution
- **Semester**: Semester 8
- **University**: FPT University

---

## 📝 License

This project is developed for educational and entrepreneurship purposes as part of the FPT University EXE201 program.
