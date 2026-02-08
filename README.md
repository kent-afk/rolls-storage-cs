## Технологический стек
- .NET 8.0 (Web API)
- Entity Framework Core 8.0
- PostgreSQL / SQLite
- Docker & Docker Compose
- xUnit (тестирование)

## Запуск проекта

### Через Docker 
1. Перейдите в корневую директорию проекта
2. Запустите проект:

```bash
docker-compose up --build
```

API будет доступен по адресу: `http://localhost:8080`

База данных PostgreSQL будет доступна по адресу: `localhost:5432`

### Локально с SQLite

1. Установите .NET SDK 8.0
2. Перейдите в директорию API:
```bash
cd src/API
```
3. Запустите проект:
```bash
dotnet run
```

API будет доступен по адресу: `http://localhost:5000`

### Локально с PostgreSQL

1. Установите PostgreSQL
2. Настройте connection string в `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=rollsdb;Username=postgres;Password=your_password"
  }
}
```
3. Запустите проект:
```bash
cd src/API
dotnet run
```

## Запуск тестов

```bash
cd WebApplication1.Tests
dotnet test
```

## API

### Добавление рулона
```http
POST /api/rolls
Content-Type: application/json

{
  "length": 100.5,
  "weight": 50.25
}
```

### Удаление рулона
```http
DELETE /api/rolls/{id}
```

### Получение списка рулонов
```http
GET /api/rolls
```

С фильтрацией:
```http
GET /api/rolls?weight.min=10&weight.max=100&length.min=50&addTime.from=2024-01-01&addTime.to=2024-12-31
```

### Получение статистики
```http
POST /api/rolls/statistics
Content-Type: application/json

{
  "from": "2024-01-01T00:00:00Z",
  "to": "2024-12-31T23:59:59Z"
}
```

## Настройка через ENV переменные

Вы можете настроить подключение к БД через переменные окружения:

```bash
export ConnectionStrings__DefaultConnection="Host=postgres;Database=rollsdb;Username=postgres;Password=postgres"
```

В Docker это настраивается в `docker-compose.yml`.

## Локальная разработка

Для локальной разработки с SQLite (по умолчанию) используется файл базы данных `RollStorage.db`.

Для использования PostgreSQL измените connection string в `appsettings.json` или через ENV переменные.
