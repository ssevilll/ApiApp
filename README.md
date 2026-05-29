# TicketApi

A .NET 8 Web API for managing events, organizers, and tickets — with JWT authentication, file uploads, email confirmation, and a dedicated xUnit test project.

---

## Project Structure

```
TicketApi/
├── ApiApp/                         # Main API project
│   ├── Controllers/
│   │   ├── AccountController.cs    # Register, login, email confirm, password reset
│   │   ├── EventsController.cs     # CRUD for events + banner upload
│   │   ├── OrganizersController.cs # CRUD for organizers + logo upload + events sub-resource
│   │   ├── TicketsController.cs    # CRUD for tickets
│   │   └── BaseController.cs       # Shared [ApiController] / [Route] base
│   ├── Data/
│   │   ├── ApiAppDbContext.cs
│   │   ├── Configurations/         # EF Fluent API configs
│   │   └── Migrations/
│   ├── DTOs/
│   │   ├── EventDtos/
│   │   ├── OrganizerDtos/
│   │   ├── TicketDtos/
│   │   └── UserDtos/
│   ├── Interfaces/
│   │   └── IFileService.cs
│   ├── Models/
│   │   ├── AppUser.cs
│   │   ├── Event.cs
│   │   ├── Organizer.cs
│   │   ├── Ticket.cs
│   │   └── Common/BaseEntity.cs
│   ├── Profile/
│   │   └── MappingProfile.cs       # AutoMapper profiles
│   ├── Services/
│   │   ├── EmailServices.cs
│   │   ├── FileService.cs
│   │   └── JWTService.cs
│   ├── Validation/
│   │   ├── Attributes/             # Custom data annotation attributes
│   │   └── Validators/             # FluentValidation validators
│   ├── Program.cs
│   ├── ServiceRegistration.cs
│   └── appsettings.json
│
└── Event.Tests/                    # xUnit test project
    ├── EventsControllerTests.cs    # Tests for EventsController
    ├── OrganizersControllerTests.cs# Tests for OrganizersController  ← CREATE THIS
    └── Helpers.cs                  # DbContextFactory + MapperFactory
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server (or LocalDB for development)
- An SMTP server (or MailKit-compatible service) for email features

---

## Getting Started

### 1. Clone and restore

```bash
git clone <repo-url>
cd TicketApi
dotnet restore
```

### 2. Configure the database

Edit `ApiApp/appsettings.Development.json` and set your connection string:

```json
{
  "ConnectionStrings": {
    "Default": "Server=.;Database=TicketApiDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 3. Apply migrations

```bash
cd ApiApp
dotnet ef database update
```

### 4. Run the API

```bash
dotnet run --project ApiApp
```

The API will be available at `https://localhost:7xxx` (check your `launchSettings.json`). Swagger UI is enabled in development mode.

---

## Running Tests

The test project uses **xUnit**, **Moq**, and **EF Core InMemory**. No database setup is needed.

```bash
dotnet test Event.Tests
```

To see verbose output:

```bash
dotnet test Event.Tests --logger "console;verbosity=detailed"
```

---

## API Overview

### Authentication (`/api/account`)

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/register` | Public | Register a new user |
| POST | `/login` | Public | Login, receive JWT + refresh token |
| POST | `/refresh` | Public | Refresh JWT |
| GET | `/confirm-email` | Public | Confirm email via link |
| POST | `/forgot-password` | Public | Send password reset email |
| POST | `/reset-password` | Public | Reset password with token |

### Organizers (`/api/organizers`)

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/` | Public | List all organizers |
| GET | `/{id}` | Public | Get organizer by ID |
| POST | `/` | Admin | Create organizer (with optional logo) |
| PUT | `/{id}` | Admin | Update organizer details |
| DELETE | `/{id}` | Admin | Delete organizer (also deletes logo file) |
| POST | `/{id}/logo` | Admin | Upload / replace organizer logo |
| GET | `/{id}/events` | Public | List all events for an organizer |

### Events (`/api/events`)

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/` | Public | List all events |
| GET | `/{id}` | Public | Get event by ID |
| POST | `/` | Admin | Create event (with optional banner) |
| PUT | `/{id}` | Admin | Update event |
| DELETE | `/{id}` | Admin | Delete event |
| POST | `/{id}/banner` | Admin | Upload / replace event banner |

### Tickets (`/api/tickets`)

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/` | Public | List all tickets |
| GET | `/{id}` | Public | Get ticket by ID |
| POST | `/` | Authenticated | Purchase a ticket |
| PUT | `/{id}` | Admin | Update ticket |
| DELETE | `/{id}` | Admin | Delete ticket |

---


## Key Libraries

| Package | Purpose |
|---------|---------|
| `Microsoft.EntityFrameworkCore.SqlServer` | Database ORM |
| `Microsoft.AspNetCore.Identity` | User management |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT middleware |
| `AutoMapper` | DTO ↔ entity mapping |
| `FluentValidation.AspNetCore` | Input validation |
| `MailKit` / `MimeKit` | Email sending |
| `Swashbuckle.AspNetCore` | Swagger / OpenAPI |
| `xunit` + `Moq` + `EF InMemory` | Unit testing |

---

## Notes

- File uploads (logos, banners) are stored under `wwwroot/uploads/` and served as static files.
- JWT tokens are short-lived; use the `/refresh` endpoint with your refresh token to get a new one.
- The `admin` role must be assigned manually (via seeding or direct DB insert) as there is no admin-registration endpoint.
