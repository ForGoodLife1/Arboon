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

---

## Testing Webhooks Locally

To test webhooks without deploying your API to the internet, you can use services like [webhook.site](https://webhook.site/) or [ngrok](https://ngrok.com/).

### Option A: Using Webhook.site
1. Go to [webhook.site](https://webhook.site/) and copy your unique URL.
2. Register the endpoint in Arboon using the seller's JWT token:
   ```bash
   curl -X POST https://localhost:5001/api/v1/webhooks \
     -H "Authorization: Bearer YOUR_TOKEN_HERE" \
     -H "Content-Type: application/json" \
     -d '{
       "url": "https://webhook.site/YOUR-UUID-HERE",
       "secret": "my_super_secret_key",
       "events": ["escrow.created", "escrow.funded", "escrow.released", "dispute.opened"]
     }'
   ```
3. Trigger an event (like creating an escrow) and watch the payload arrive in real-time on webhook.site.

### Option B: Using ngrok
1. Run a local application on a specific port (e.g., `3000`).
2. Expose it using ngrok: `ngrok http 3000`
3. Copy the forwarding HTTPS URL from ngrok and register it via the `/api/v1/webhooks` endpoint.

### Verifying Webhook Signatures

The Arboon API sends an `Arboon-Signature` header (HMAC-SHA256) with every webhook delivery so you can verify the payload was genuinely sent by Arboon.

**Example: Node.js (Express)**
```javascript
const crypto = require('crypto');
const express = require('express');
const app = express();

app.post('/webhook', express.json(), (req, res) => {
  const signatureHeader = req.headers['arboon-signature'];
  const eventName = req.headers['arboon-event'];
  const secret = 'my_super_secret_key'; // Same secret you used when registering

  // 1. Get the raw hash from the header (format is 'sha256={hash}')
  const hash = signatureHeader.replace('sha256=', '');

  // 2. The string to hash: "arboon.{eventName}.{rawPayloadBody}"
  const payloadString = JSON.stringify(req.body);
  const dataToHash = `arboon.${eventName}.${payloadString}`;

  // 3. Compute hash using HMAC SHA256
  const hmac = crypto.createHmac('sha256', secret);
  hmac.update(dataToHash);
  const computedHash = hmac.digest('hex');

  if (computedHash === hash) {
    console.log('Valid signature! Processing webhook...', req.body);
    res.status(200).send('OK');
  } else {
    console.error('Invalid signature!');
    res.status(401).send('Unauthorized');
  }
});
```

**Example: C# (ASP.NET Core)**
```csharp
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

[HttpPost("webhook")]
public async Task<IActionResult> ReceiveWebhook()
{
    var signatureHeader = Request.Headers["Arboon-Signature"].ToString();
    var eventName = Request.Headers["Arboon-Event"].ToString();
    var secret = "my_super_secret_key";
    
    using var reader = new StreamReader(Request.Body);
    var body = await reader.ReadToEndAsync();
    
    var hash = signatureHeader.Replace("sha256=", "");
    var dataToHash = $"arboon.{eventName}.{body}";
    
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
    var computedHash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToHash))).Replace("-", "").ToLowerInvariant();
    
    if (computedHash == hash)
    {
        // Valid webhook!
        var payload = JsonSerializer.Deserialize<JsonElement>(body);
        return Ok();
    }
    
    return Unauthorized();
}
```

---

## Example Dispute Flow

Disputes pause an escrow (status becomes `DISPUTED`) and require an admin to resolve. Here is the flow using `curl`.

### 1. Buyer Opens a Dispute
The buyer needs the `buyer_token` (from the payment cookie or directly).
```bash
curl -X POST https://localhost:5001/api/v1/disputes/buyer \
  -H "Content-Type: application/json" \
  -d '{
    "escrowId": "esc_123456789",
    "reason": "The delivered work does not match the requirements.",
    "buyerToken": "33333333-3333-3333-3333-333333333333"
  }'
```
*(This triggers the `dispute.opened` webhook)*

### 2. Seller and Buyer Discuss
**Seller replies:**
```bash
curl -X POST https://localhost:5001/api/v1/disputes/YOUR_DISPUTE_ID/messages \
  -H "Authorization: Bearer SELLER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "message": "I provided all requested revisions as per our agreement."
  }'
```

**Buyer replies:**
```bash
curl -X POST https://localhost:5001/api/v1/disputes/YOUR_DISPUTE_ID/messages \
  -H "Content-Type: application/json" \
  -d '{
    "message": "No, the logo is completely different from the brief.",
    "buyerToken": "33333333-3333-3333-3333-333333333333"
  }'
```

### 3. Admin Resolves the Dispute
An admin reviews the messages and resolves the dispute. They can choose `ReleasedToSeller` or `RefundedToBuyer`.

First, login as the admin (email: `admin@arboon.app`, password: `Admin@123`) to get the admin token.
```bash
curl -X POST https://localhost:5001/api/v1/admin/disputes/YOUR_DISPUTE_ID/resolve \
  -H "Authorization: Bearer ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "resolution": "RefundedToBuyer",
    "adminNote": "The seller failed to deliver the agreed-upon design. Refund issued."
  }'
```
*(This updates the escrow status to `REFUNDED` and triggers the `dispute.resolved` webhook)*
