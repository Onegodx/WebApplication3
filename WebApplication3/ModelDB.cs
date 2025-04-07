using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

public class ModelDB : DbContext
{
    public ModelDB(DbContextOptions<ModelDB> options) : base(options) { }
    public DbSet<Tariff> Tariffs { get; set; }
    public DbSet<Call> Calls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Заполнение тарифов
        modelBuilder.Entity<Tariff>().HasData(
            new Tariff { CityCode = 1, CityName = "Москва", PricePerMinute = 5.5m },
            new Tariff { CityCode = 2, CityName = "Санкт-Петербург", PricePerMinute = 4.8m }
                // ... еще 5 тарифов
        );

        // Заполнение переговоров
        modelBuilder.Entity<Call>().HasData(
            new Call
            {
                Id = 1,
                OwnerFIO = "Иванов И.И.",
                CityCode = 1,
                CityName = "Москва",
                DurationMinutes = 15,
                Cost = 82.5m,
                CallDate = new DateTime(2023, 10, 5)
            }
                // ... еще 14 записей
        );
    }
}