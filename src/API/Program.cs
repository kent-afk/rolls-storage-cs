using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebApplication1.Core.Data;
using WebApplication1.Services.Implementation;
using WebApplication1.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Get connection string from configuration (supports appsettings.json and environment variables)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=RollStorage.db";

// Register database context
builder.Services.AddDbContext<RollsDbContext>(options =>
{
    // Use SQLite by default, can be switched to PostgreSQL via configuration
    var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider")?.ToLower() ?? "sqlite";
    
    if (databaseProvider == "postgresql")
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// Register repository and service
builder.Services.AddScoped<IRollRepository, RollRepository>();
builder.Services.AddScoped<IRollService, RollService>();

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Metal Roll Warehouse API",
        Description = "RESTful API for managing metal rolls in a warehouse",
        Version = "v1",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@warehouse.local"
        }
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS support
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p
    => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Initialize database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RollsDbContext>();
    try
    {
        // Ensure database is created (works for both SQLite and PostgreSQL)
        context.Database.EnsureCreated();
        app.Logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred initializing the database");
    }
}

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Metal Roll Warehouse API v1");
        c.RoutePrefix = "swagger"; // Access swagger at /swagger
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("Metal Roll Warehouse API starting...");

app.Run();

public partial class Program { }
