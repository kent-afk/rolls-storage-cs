# Roll Storage API

A REST API for managing industrial rolls, built with **C#**, **.NET 8.0**, and **ASP.NET Core Web API**.  
The project uses **Entity Framework Core 8.0** with **PostgreSQL** and **SQLite**, and is containerized with **Docker & Docker Compose**.  
Unit tests are written with **xUnit**.

***

## Technologies & Stack

- **.NET 8.0** (Web API)  
- **Entity Framework Core 8.0**  
- **PostgreSQL** and **SQLite**  
- **Docker & Docker Compose** (for containers)  
- **xUnit** (unit and integration tests)

***

## Running the Project

### With Docker (recommended)

1. Go to the project root directory.
2. Run:
   ```bash
   docker-compose up --build
   ```
3. The API will be available at:  
   `http://localhost:8080`  
4. The PostgreSQL database will be available at:  
   `localhost:5432`

***

### Locally with SQLite

1. Install **.NET SDK 8.0**.  
2. Go to the API folder:
   ```bash
   cd src/API
   ```
3. Run:
   ```bash
   dotnet run
   ```
4. The API will be available at:  
   `http://localhost:5000`

By default, the project uses **SQLite** with a local database file `RollStorage.db`.

***

### Locally with PostgreSQL

1. Install **PostgreSQL** and run the server.  
2. Configure the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=rollsdb;Username=postgres;Password=your_password"
     }
   }
   ```
3. Go to the API folder:
   ```bash
   cd src/API
   ```
4. Run:
   ```bash
   dotnet run
   ```

***

## Environment Variables (Container Configuration)

You can configure the database connection through environment variables:

```bash
export ConnectionStrings__DefaultConnection="Host=postgres;Database=rollsdb;Username=postgres;Password=postgres"
```

In Docker, this is set inside `docker-compose.yml`.

***

## API Endpoints

### Add a roll

`POST /api/rolls`  
Content-Type: `application/json`

```json
{
  "length": 100.5,
  "weight": 50.25
}
```

***

### Delete a roll

`DELETE /api/rolls/{id}`

***

### Get list of rolls

`GET /api/rolls`

With filters:  
`GET /api/rolls?weight.min=10&weight.max=100&length.min=50&addTime.from=2024-01-01&addTime.to=2024-12-31`

***

### Get statistics

`POST /api/rolls/statistics`  
Content-Type: `application/json`

```json
{
  "from": "2024-01-01T00:00:00Z",
  "to": "2024-12-31T23:59:59Z"
}
```

***

## Running Tests

```bash
cd WebApplication1.Tests
dotnet test
```

***

## Local Development Setup

- For local development with **SQLite** (the default) the project uses the file `RollStorage.db`.  
- To switch to **PostgreSQL**, edit the `ConnectionStrings:DefaultConnection` in `appsettings.json` or set the corresponding environment variable.

***
