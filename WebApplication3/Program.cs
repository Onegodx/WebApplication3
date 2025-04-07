using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Добавляем контекст базы данных
builder.Services.AddDbContext<ModelDB>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем авторизацию и аутентификацию (JWT)
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Настройка конвейера HTTP-запросов
app.UseAuthentication();
app.UseAuthorization();

// Эндпоинты
app.MapGet("/api/invoice", [Authorize] async (string fio, DateTime date, ModelDB db) =>
{
    var calls = await db.Calls
        .Where(c => c.OwnerFIO == fio && c.CallDate.Date == date.Date)
        .ToListAsync(); // Теперь ToListAsync доступен

    decimal total = calls.Sum(c => c.Cost);

    return Results.Ok(new
    {
        FIO = fio,
        Date = date,
        TotalAmount = total,
        Calls = calls
    });
});

app.MapGet("/api/report", [Authorize] async (int cityCode, DateTime date, ModelDB db) =>
{
    var calls = await db.Calls
        .Where(c => c.CityCode == cityCode && c.CallDate.Date == date.Date)
        .ToListAsync(); // Теперь ToListAsync доступен

    return Results.Ok(new
    {
        City = calls.FirstOrDefault()?.CityName,
        Date = date,
        TotalCalls = calls.Count,
        TotalDuration = calls.Sum(c => c.DurationMinutes),
        TotalCost = calls.Sum(c => c.Cost)
    });
});

app.Run();