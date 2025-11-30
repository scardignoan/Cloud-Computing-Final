# Cloud Computing Final Project - Book API

## Project Overview
This project upgrades the midterm Azure Functions API into a production-ready cloud solution. It provides CRUD operations for books stored in Azure SQL via Entity Framework Core, secures every endpoint with Key Vault-backed API keys, and includes a validation workflow that archives old books through a Logic App trigger. Observability is handled by Application Insights, and an Azure Dashboard summarizes API health, SQL usage, Key Vault access, and Logic App runs.

Key Features:
- Azure Functions (.NET 8 isolated) HTTP endpoints for Create, Read, Update, Delete, Purge, Count, and Validate.
- Persistent storage in Azure SQL with EF Core migrations.
- API key authentication retrieved from Azure Key Vault using DefaultAzureCredential.
- Logic App: Recurrence trigger ? Key Vault secret ? PATCH /api/books/validate ? notification.
- Application Insights telemetry + custom Azure Dashboard.

## Setup Instructions
1. **Clone the repo**
   ```bash
   git clone https://github.com/scardignoan/Cloud-Computing-Final.git
   cd Cloud-Computing-Final
   ```

2. **Install tools**
   - .NET 8 SDK
   - Azure Functions Core Tools v4
   - Azure CLI

3. **Configure local secrets** (stored via user-secrets, not committed):
   ```bash
   dotnet user-secrets set API_KEY "<local-key>"
   dotnet user-secrets set SqlConnectionString "<local-sql-connection-string>"
   ```
   `local.settings.json` contains placeholders and is ignored by Git.

4. **Apply migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run locally**
   ```bash
   func start
   ```
   Use Thunder Client/Postman with header `X-API-Key: <local-key>`.

6. **Deploy to Azure**
   ```bash
   func azure functionapp publish anthonyscardigno-midterm-functions
   ```
   Ensure Function App settings include `KeyVaultName`, `ApiKeySecretName`, and `SqlConnectionStringSecretName`, pointing to secrets in Key Vault.

## API Reference
All endpoints require header `X-API-Key`. Base URL is `https://<function-app>.azurewebsites.net`.

| Method | Route | Description | Body – Request | Response |
|--------|-------|-------------|----------------|----------|
| POST   | `/api/books`          | Create book           | JSON Book fields | 201 + book JSON |
| GET    | `/api/books`          | List books            | none             | 200 + array of books |
| GET    | `/api/books/{id}`     | Get by Id             | none             | 200 + book or 404 |
| PUT    | `/api/books/{id}`     | Update book           | JSON Book fields | 200 + updated book |
| DELETE | `/api/books/{id}`     | Delete book           | none             | 204 or 404 |
| DELETE | `/api/books`          | Purge all             | none             | 204 |
| GET    | `/api/books/count`    | Count books           | none             | 200 + `{ "count": n }` |
| PATCH  | `/api/books/validate` | Batch validation      | none             | 200 + `{ "updatedCount": n, "timestamp": ... }` |

**Book JSON Example**
```json
{
  "title": "Example",
  "author": "Jane Doe",
  "isbn": "9781234567890",
  "publisher": "TechPress",
  "year": 2020,
  "description": "Cloud-native reference"
}
```

**Validation Logic**
- Books older than 10 years are marked `archived=true` and `validatedOn=<timestamp>`.
- Response includes `updatedCount` and UTC timestamp.

**Logic App Automation**
- Recurrence trigger (or manual)
- Action: Get secret (`ApiKey`) from Key Vault using Logic App managed identity
- Action: HTTP PATCH `/api/books/validate` with `x-api-key` header
- Action: Send notification (email/Teams) with run status

### Troubleshooting
- **401 Unauthorized**: confirm `X-API-Key` matches Key Vault secret / Function App config.
- **500 errors**: check Application Insights; usually missing `SqlConnectionString` secret or SQL firewall.
- **Database access**: add client IP to SQL server firewall.

Feel free to open issues or contribute improvements.


flowchart TD
    client[Client (Thunder/Logic App/Browser)] -->|HTTPS + X-API-Key| func[Azure Function App (Book API)]
    func -->|EF Core| sql[(Azure SQL Database)]
    func -->|SecretClient / DefaultAzureCredential| kv[Azure Key Vault]
    kv ---|Secrets| kvSecrets[(ApiKey, SqlConnectionString)]
    func --> ai[(Application Insights)]
    logic[Logic App\n(Recurrence → Get Secret → PATCH /validate → Notify)] -->|HTTP PATCH /api/books/validate\nx-api-key from Key Vault| func
    logic --> kv
    logic --> ai
    dash[Azure Dashboard] --> ai
    dash --> sql
    dash --> kv
    dash --> logic
    subgraph Infra
        func
        sql
        kv
        ai
        logic
        dash
    end

