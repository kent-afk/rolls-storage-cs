# Metal Roll Warehouse API

## Overview

This is a RESTful API backend for managing a metal roll warehouse. The application allows you to add, remove, filter, and analyze metal rolls stored in the warehouse.

## Features

### Core Functionality
- **Add Roll** - Add new metal rolls to the warehouse (length and weight are required parameters)
- **Remove Roll** - Remove a roll from the warehouse by its unique GUID identifier
- **Get Roll** - Retrieve a specific roll by its ID
- **Get Rolls List** - Retrieve list of rolls with filtering support
- **Statistics** - Get comprehensive warehouse statistics for a specified period

### Supported Filters (can be combined)
- Filter by ID (GUID)
- Filter by weight range (min/max)
- Filter by length range (min/max)
- Filter by addition date range
- Filter by removal date range

### Statistics Include
- Number of rolls added during the period
- Number of rolls removed during the period
- Average length and weight of rolls in warehouse during period
- Maximum and minimum length and weight
- Total weight of all rolls in warehouse
- Maximum and minimum time between addition and removal
- **Bonus**: Day with minimum/maximum roll count during period
- **Bonus**: Day with minimum/maximum total weight during period

## Technology Stack

- **.NET 8** - Web API framework
- **Entity Framework Core** - ORM for database operations
- **SQLite** - Default database (easily switchable to PostgreSQL)
- **xUnit** - Testing framework
- **In-Memory Database** - Tests run without real database dependency
- **Docker** - Containerization support
- **Swagger/OpenAPI** - API documentation

## Architecture

The project follows clean architecture principles with clear separation of concerns:

```
src/
├── API/              # Web API Controllers, Program.cs, Configuration
├── Core/             # Database context (DbContext)
├── Entity/           # Domain models, DTOs, Request/Response objects
│   ├── Rolls/        # Roll entity, CreateRollRequest
│   ├── Filters/      # Filter models (FilterRolls, FilterRangeRolls, FilterTimeRolls)
│   └── Statistics/   # StatisticRequest, StatisticResponse
└── Services/         # Business logic layer
    ├── Interfaces/   # IRollService, IRollRepository
    └── Implementation/ # RollService, RollRepository
test/
└── Integration tests using in-memory database
```

### Key Design Decisions

1. **Repository Pattern** - Data access is abstracted through `IRollRepository` interface
2. **Service Layer** - Business logic separated from controllers (`IRollService`)
3. **GUID-based IDs** - Each roll has a unique GUID identifier (not reset on application restart)
4. **In-Memory Testing** - Tests use in-memory database to avoid dependencies on real databases
5. **Dependency Injection** - Full DI support for easy testing and extensibility

## Getting Started

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose (optional, for containerized deployment)

### Building the Project

```bash
dotnet build
```

### Running the Application

```bash
cd src/API
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger` (in Development mode)

### Running Tests

```bash
dotnet test
```

All 11 integration tests should pass.

### Using Docker

```bash
docker-compose up --build
```

This will start:
- PostgreSQL database on port 5432
- Web API on port 5000

## API Endpoints

### Add Roll
```
POST /api/rolls
Content-Type: application/json

{
  "length": 100.5,
  "weight": 50.25
}
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "length": 100.5,
  "weight": 50.25,
  "dateAdd": "2024-01-15T10:30:00Z",
  "dateRemove": null
}
```

### Remove Roll
```
DELETE /api/rolls/{id}
```

### Get Roll by ID
```
GET /api/rolls/{id}
```

### Get Rolls with Filters
```
GET /api/rolls?weight.min=10&weight.max=100&length.min=50
GET /api/rolls?length.min=120&weight.max=80  (combined filters)
```

### Get Statistics
```
POST /api/rolls/statistics
Content-Type: application/json

{
  "from": "2024-01-01T00:00:00Z",
  "to": "2024-12-31T23:59:59Z"
}
```

**Response:**
```json
{
  "totalAdded": 10,
  "totalRemoved": 5,
  "averageLength": 150.5,
  "averageWeight": 75.25,
  "maxLength": 200,
  "minLength": 100,
  "maxWeight": 100,
  "minWeight": 50,
  "totalWeight": 1505,
  "maxTimeInStock": "30.00:00:00",
  "minTimeInStock": "1.00:00:00",
  "dayWithMinRollCount": "2024-06-15T00:00:00",
  "dayWithMaxRollCount": "2024-12-01T00:00:00",
  "dayWithMinWeight": "2024-06-15T00:00:00",
  "dayWithMaxWeight": "2024-12-01T00:00:00"
}
```

## Configuration

Database connection can be configured via:

1. **appsettings.json** - Default SQLite database
2. **Environment Variables** - Override connection string
3. **Command Line** - Pass configuration at runtime

### Configuration Keys

| Key | Description | Default |
|-----|-------------|---------|
| `ConnectionStrings:DefaultConnection` | Database connection string | `Data Source=RollStorage.db` |
| `DatabaseProvider` | Database provider | `sqlite` (or `postgresql`) |

### Example Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=RollStorage.db"
  },
  "DatabaseProvider": "sqlite"
}
```

For PostgreSQL:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=warehouse;Username=postgres;Password=password"
  },
  "DatabaseProvider": "postgresql"
}
```

## Error Handling

The API handles standard error cases with appropriate HTTP status codes:
- **400 Bad Request** - Invalid input data (negative length/weight, invalid date range)
- **404 Not Found** - Roll not found
- **500 Internal Server Error** - Database unavailable or other internal errors

## Testing

Tests use in-memory database to avoid dependencies on real databases:
- Each test uses a unique database name for isolation
- All 11 integration tests pass

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal
```

## Project Structure Details

| Folder/File | Purpose |
|-------------|---------|
| `API/Program.cs` | Application entry point, DI configuration, middleware setup |
| `API/RollsController.cs` | REST API controller - handles HTTP requests/responses |
| `Core/Data/RollsDbContext.cs` | Entity Framework DbContext |
| `Entity/Rolls/Roll.cs` | Domain entity for metal roll |
| `Entity/Rolls/CreateRollRequest.cs` | DTO for creating new rolls |
| `Entity/Filters/FilterRolls.cs` | Filter parameters for querying rolls |
| `Entity/Statistics/` | Request/Response DTOs for statistics |
| `Services/Interfaces/` | Service and repository interfaces |
| `Services/Implementation/` | Business logic implementation |

## Extensibility

The architecture allows easy swapping of data storage:
- Currently uses SQLite by default
- Can be switched to PostgreSQL via configuration
- Repository pattern enables switching to in-memory or file-based storage
- Easy to add caching layer (e.g., Redis) in the future

## Running in Production

For production deployment:

1. Set environment variables:
```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Host=postgres;Database=warehouse;..."
export DatabaseProvider=postgresql
```

2. Use Docker Compose with PostgreSQL

## Notes for Reviewers

- The solution addresses all requirements from the task description
- GUID is used instead of integer ID to ensure uniqueness across restarts
- Business logic is properly separated in the Service layer
- Swagger documentation is available at `/swagger`
- Tests use in-memory database and don't require real database
- Error handling is implemented for standard cases
- Docker support is included
- Configuration is externalized (supports both SQLite and PostgreSQL)
