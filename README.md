# Arboon (عَرْبُون) — Micro-Escrow Platform API

Arboon is a micro-escrow backend service built with ASP.NET Core 10, following Clean Architecture principles. It provides a secure financial guarantee platform for freelancers and small businesses.

## Features
- **Clean Architecture:** Divided into `Domain`, `Application`, `Infrastructure`, and `API` layers.
- **Micro-Escrow State Machine:** Secure transitions (`PENDING` -> `FROZEN` -> `RELEASED` / `CANCELLED`).
- **Guest Checkout (Magic Link):** Buyers can pay and receive a Magic Link via email to release funds securely without creating an account.
- **JWT Authentication:** Secure seller endpoints.
- **Global Error Handling:** Consistent JSON error responses across the API.
- **Entity Framework Core:** Uses PostgreSQL with decimal precision for all financial calculations.

## Requirements
- .NET 10 SDK
- PostgreSQL 15+

## Getting Started

1. Clone the repository.
2. Ensure PostgreSQL is running. The default connection string in `appsettings.Development.json` is:
   `Host=localhost;Port=5432;Database=arboon_dev;Username=postgres;Password=postgres`
3. Apply database migrations:
   ```bash
   dotnet ef database update --project src/Arboon.Infrastructure --startup-project src/Arboon.API
   ```
4. Run the API:
   ```bash
   dotnet run --project src/Arboon.API
   ```
5. Swagger documentation is available at: `https://localhost:5001/swagger` (or the URL assigned by Kestrel).

---

## API Testing Guide (Curl Examples)

### 1. Register a Seller Account
```bash
curl -X POST https://localhost:5001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name": "Ahmed", "email": "ahmed@example.com", "password": "Password@123"}'
```

### 2. Login
```bash
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "ahmed@example.com", "password": "Password@123"}'
```
*(Copy the `token` from the response for the following requests)*

### 3. Create an Escrow (Seller)
```bash
curl -X POST https://localhost:5001/api/v1/escrows \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "تصميم موقع إلكتروني",
    "amount": 500.00,
    "currency": "USD",
    "buyerEmail": "buyer@example.com",
    "conditions": "تسليم الموقع خلال 10 أيام"
  }'
```
*(Copy the `id` of the created escrow)*

### 4. Pay the Escrow (Buyer - Guest)
This endpoint simulates the payment and generates the `buyer_token` via an HTTP-only cookie. In development mode, the email containing the Magic Link is logged to the console.
```bash
curl -X POST https://localhost:5001/api/v1/escrows/YOUR_ESCROW_ID/pay \
  -H "Content-Type: application/json" \
  -d '{"cardToken": "tok_visa"}'
```

### 5. Check Buyer Authorization Status
To see if you are authorized to release the funds (checks the HttpOnly cookie or query param).
```bash
curl -X GET "https://localhost:5001/api/v1/escrows/YOUR_ESCROW_ID/buyer-status?token=OPTIONAL_TOKEN"
```

### 6. Release Funds (Buyer)
Releases the frozen funds to the seller's wallet. The buyer's token is read from the `arboon_buyer_token` HttpOnly cookie automatically.
```bash
curl -X POST https://localhost:5001/api/v1/escrows/YOUR_ESCROW_ID/release \
  -H "Content-Type: application/json" \
  -d '{}'
```

### 7. View Seller Wallet Balance
```bash
curl -X GET https://localhost:5001/api/v1/wallet/balance \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## Architecture Overview
- **Domain:** Contains core entities (`User`, `Escrow`, `Wallet`, `BuyerToken`), enums, and domain exceptions. Business logic (e.g., fee calculation, state transitions) resides here.
- **Application:** Contains DTOs, Validation rules (FluentValidation), and core services (`AuthService`, `EscrowService`, `WalletService`). Uses abstractions (`IAppDbContext`, `IEmailService`, etc.).
- **Infrastructure:** Implements abstractions. Includes `AppDbContext` (EF Core), configurations, migrations, JWT generation, and mock external services (Email, Payment).
- **API:** Controllers, Swagger configuration, and global exception middleware to format domain exceptions into standard JSON error responses.
