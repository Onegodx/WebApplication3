using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;



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
// 1. Получить все тарифы
app.MapGet("/api/tariffs", [Authorize] async (ModelDB db) =>
    await db.Tariffs.ToListAsync());

// 2. Получить тариф по коду города
app.MapGet("/api/tariffs/{cityCode}", [Authorize] async (int cityCode, ModelDB db) =>
{
    var tariff = await db.Tariffs.FirstOrDefaultAsync(t => t.CityCode == cityCode);
    return tariff is null ? Results.NotFound() : Results.Ok(tariff);
});

// 3. Создать новый тариф
app.MapPost("/api/tariffs", [Authorize] async (Tariff tariff, ModelDB db) =>
{
    db.Tariffs.Add(tariff);
    await db.SaveChangesAsync();
    return Results.Created($"/api/tariffs/{tariff.CityCode}", tariff);
});

// 4. Обновить тариф
app.MapPut("/api/tariffs/{cityCode}", [Authorize] async (int cityCode, Tariff updatedTariff, ModelDB db) =>
{
    var tariff = await db.Tariffs.FirstOrDefaultAsync(t => t.CityCode == cityCode);
    if (tariff is null) return Results.NotFound();

    tariff.CityName = updatedTariff.CityName;
    tariff.PricePerMinute = updatedTariff.PricePerMinute;
    await db.SaveChangesAsync();
    return Results.Ok(tariff);
});

// 5. Удалить тариф
app.MapDelete("/api/tariffs/{cityCode}", [Authorize] async (int cityCode, ModelDB db) =>
{
    var tariff = await db.Tariffs.FirstOrDefaultAsync(t => t.CityCode == cityCode);
    if (tariff is null) return Results.NotFound();

    db.Tariffs.Remove(tariff);
    await db.SaveChangesAsync();
    return Results.Ok(tariff);
});

// 6. Получить все звонки
app.MapGet("/api/calls", [Authorize] async (ModelDB db) =>
    await db.Calls.ToListAsync());

// 7. Получить звонки по ФИО
app.MapGet("/api/calls/owner", [Authorize] async (string fio, ModelDB db) =>
    await db.Calls.Where(c => c.OwnerFIO == fio).ToListAsync());

// 8. Создать новый звонок
app.MapPost("/api/calls", [Authorize] async (Call call, ModelDB db) =>
{
    // Автоматически рассчитываем стоимость
    var tariff = await db.Tariffs.FirstOrDefaultAsync(t => t.CityCode == call.CityCode);
    if (tariff is null) return Results.BadRequest("Тариф не найден");

    call.Cost = call.DurationMinutes * tariff.PricePerMinute;
    call.CityName = tariff.CityName; // Синхронизируем название города

    db.Calls.Add(call);
    await db.SaveChangesAsync();
    return Results.Created($"/api/calls/{call.Id}", call);
});

// 9. Получить отчет по звонкам в город за период
app.MapGet("/api/calls/report", [Authorize] async (int cityCode, DateTime startDate, DateTime endDate, ModelDB db) =>
{
    var calls = await db.Calls
        .Where(c => c.CityCode == cityCode && c.CallDate >= startDate && c.CallDate <= endDate)
        .ToListAsync();

    return new
    {
        City = calls.FirstOrDefault()?.CityName,
        TotalCalls = calls.Count,
        TotalDuration = calls.Sum(c => c.DurationMinutes),
        TotalCost = calls.Sum(c => c.Cost)
    };
});

// 10. Удалить звонок
app.MapDelete("/api/calls/{id}", [Authorize] async (int id, ModelDB db) =>
{
    var call = await db.Calls.FirstOrDefaultAsync(c => c.Id == id);
    if (call is null) return Results.NotFound();

    db.Calls.Remove(call);
    await db.SaveChangesAsync();
    return Results.Ok(call);
});
app.Run();
