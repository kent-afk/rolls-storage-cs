using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApplication1.Core.Data;
using WebApplication1.Core.DataBase;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=RollStorage";

builder.Services.AddDbContext<RollsDbContext>(options =>
{
    if (connectionString.StartsWith("Host=") || connectionString.StartsWith("Server=") || connectionString.Contains("postgresql"))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

builder.Services.AddScoped<IDataBase, RollsDb>();

builder.Services.AddControllers();

builder.Services.AddCors(options => options.AddPolicy("AllowAll", p
    => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));


var app = builder.Build();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RollsDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Console.WriteLine("Database initialized");
    }
    catch (Exception e)
    {
        Console.WriteLine("An error occurred initializing the database");
    }
}

app.Run();

public partial class Program { }
