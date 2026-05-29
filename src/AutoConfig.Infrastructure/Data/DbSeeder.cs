using AutoConfig.Core.Entities;
using AutoConfig.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace AutoConfig.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger)
    {
        if (db.Database.IsRelational())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync()) return;

        logger.LogInformation("Seeding database...");

        var models = SeedModels();
        var motorizations = SeedMotorizations(models);
        var options = SeedOptions();

        await db.CarModels.AddRangeAsync(models.Values);
        await db.Motorizations.AddRangeAsync(motorizations.Values);
        await db.CarOptions.AddRangeAsync(options.Values);
        await db.SaveChangesAsync();

        await SeedIncompatibilitiesAsync(db, options);
        await SeedUsersAsync(db);
    }

    private static Dictionary<string, CarModel> SeedModels() => new()
    {
        ["m1"] = new() { Id = Guid.Parse("11111111-0000-0000-0000-000000000001"), Name = "Serie 3", Brand = "BMW", Category = CarCategory.Sedan, BasePrice = 42900, Description = "La berlina sportiva per eccellenza, con un equilibrio perfetto tra prestazioni e comfort.", ImageColor = "#1a1a2e" },
        ["m2"] = new() { Id = Guid.Parse("11111111-0000-0000-0000-000000000002"), Name = "Q5", Brand = "Audi", Category = CarCategory.Suv, BasePrice = 56900, Description = "SUV premium con tecnologia all'avanguardia e design raffinato.", ImageColor = "#16213e" },
        ["m3"] = new() { Id = Guid.Parse("11111111-0000-0000-0000-000000000003"), Name = "Classe C", Brand = "Mercedes", Category = CarCategory.Sedan, BasePrice = 46500, Description = "Eleganza senza compromessi con il massimo del lusso tedesco.", ImageColor = "#0f3460" },
        ["m4"] = new() { Id = Guid.Parse("11111111-0000-0000-0000-000000000004"), Name = "Golf", Brand = "Volkswagen", Category = CarCategory.Hatchback, BasePrice = 28900, Description = "L'icona del segmento compatto, affidabile e versatile.", ImageColor = "#533483" },
        ["m5"] = new() { Id = Guid.Parse("11111111-0000-0000-0000-000000000005"), Name = "Cayenne", Brand = "Porsche", Category = CarCategory.Suv, BasePrice = 89900, Description = "Il SUV sportivo definitivo, prestazioni da supercar con la praticità di un SUV.", ImageColor = "#e94560" },
    };

    private static Dictionary<string, Motorization> SeedMotorizations(Dictionary<string, CarModel> m) => new()
    {
        ["mo1"]  = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000001"), ModelId = m["m1"].Id, Name = "318i",       FuelType = FuelType.Petrol,   Power = 156, Torque = 250, Acceleration = 8.4m,  Consumption = "6.1 L/100km",  Price = 0 },
        ["mo2"]  = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000002"), ModelId = m["m1"].Id, Name = "320d",       FuelType = FuelType.Diesel,   Power = 190, Torque = 400, Acceleration = 7.1m,  Consumption = "4.5 L/100km",  Price = 2500 },
        ["mo3"]  = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000003"), ModelId = m["m1"].Id, Name = "330i",       FuelType = FuelType.Petrol,   Power = 258, Torque = 400, Acceleration = 5.8m,  Consumption = "6.9 L/100km",  Price = 6800 },
        ["mo4"]  = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000004"), ModelId = m["m1"].Id, Name = "330e",       FuelType = FuelType.Hybrid,   Power = 292, Torque = 420, Acceleration = 5.9m,  Consumption = "1.8 L/100km",  Price = 9200 },
        ["mo5"]  = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000005"), ModelId = m["m2"].Id, Name = "35 TDI",     FuelType = FuelType.Diesel,   Power = 163, Torque = 370, Acceleration = 9.0m,  Consumption = "5.2 L/100km",  Price = 0 },
        ["mo6"]  = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000006"), ModelId = m["m2"].Id, Name = "40 TDI",     FuelType = FuelType.Diesel,   Power = 204, Torque = 400, Acceleration = 7.5m,  Consumption = "5.8 L/100km",  Price = 3200 },
        ["mo7"]  = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000007"), ModelId = m["m2"].Id, Name = "45 TFSI",    FuelType = FuelType.Petrol,   Power = 265, Torque = 370, Acceleration = 5.9m,  Consumption = "7.3 L/100km",  Price = 5900 },
        ["mo8"]  = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000008"), ModelId = m["m2"].Id, Name = "55 TFSI e",  FuelType = FuelType.Hybrid,   Power = 367, Torque = 500, Acceleration = 5.3m,  Consumption = "2.2 L/100km",  Price = 12500 },
        ["mo9"]  = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000009"), ModelId = m["m3"].Id, Name = "C 180",      FuelType = FuelType.Petrol,   Power = 170, Torque = 270, Acceleration = 8.0m,  Consumption = "6.4 L/100km",  Price = 0 },
        ["mo10"] = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000010"), ModelId = m["m3"].Id, Name = "C 220 d",    FuelType = FuelType.Diesel,   Power = 200, Torque = 440, Acceleration = 7.1m,  Consumption = "4.7 L/100km",  Price = 2800 },
        ["mo11"] = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000011"), ModelId = m["m3"].Id, Name = "C 300",      FuelType = FuelType.Petrol,   Power = 258, Torque = 400, Acceleration = 6.0m,  Consumption = "7.0 L/100km",  Price = 7100 },
        ["mo12"] = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000012"), ModelId = m["m3"].Id, Name = "C 300 e",    FuelType = FuelType.Hybrid,   Power = 313, Torque = 550, Acceleration = 5.7m,  Consumption = "1.5 L/100km",  Price = 10800 },
        ["mo13"] = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000013"), ModelId = m["m4"].Id, Name = "1.0 eTSI",   FuelType = FuelType.Petrol,   Power = 110, Torque = 200, Acceleration = 10.5m, Consumption = "5.4 L/100km",  Price = 0 },
        ["mo14"] = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000014"), ModelId = m["m4"].Id, Name = "1.5 eTSI",   FuelType = FuelType.Petrol,   Power = 150, Torque = 250, Acceleration = 8.5m,  Consumption = "5.9 L/100km",  Price = 1800 },
        ["mo15"] = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000015"), ModelId = m["m4"].Id, Name = "2.0 TDI",    FuelType = FuelType.Diesel,   Power = 150, Torque = 360, Acceleration = 8.6m,  Consumption = "4.3 L/100km",  Price = 2200 },
        ["mo16"] = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000016"), ModelId = m["m4"].Id, Name = "GTE",        FuelType = FuelType.Hybrid,   Power = 245, Torque = 400, Acceleration = 6.7m,  Consumption = "1.4 L/100km",  Price = 8500 },
        ["mo17"] = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000017"), ModelId = m["m5"].Id, Name = "V6 3.0T",    FuelType = FuelType.Petrol,   Power = 340, Torque = 450, Acceleration = 6.2m,  Consumption = "9.4 L/100km",  Price = 0 },
        ["mo18"] = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000018"), ModelId = m["m5"].Id, Name = "S V8 2.9T",  FuelType = FuelType.Petrol,   Power = 440, Torque = 550, Acceleration = 5.0m,  Consumption = "10.6 L/100km", Price = 18500 },
        ["mo19"] = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000019"), ModelId = m["m5"].Id, Name = "E-Hybrid",   FuelType = FuelType.Hybrid,   Power = 470, Torque = 700, Acceleration = 4.7m,  Consumption = "3.2 L/100km",  Price = 22000 },
        ["mo20"] = new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000020"), ModelId = m["m5"].Id, Name = "Turbo V8",   FuelType = FuelType.Petrol,   Power = 650, Torque = 800, Acceleration = 3.7m,  Consumption = "12.4 L/100km", Price = 56000 },
    };

    private static Dictionary<string, CarOption> SeedOptions() => new()
    {
        ["opt1"]  = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000001"), Name = "Bianco Alpine",          Description = "Bianco metallizzato brillante",                          Category = OptionCategory.Color,      Price = 0,    Color = "#F5F5F5" },
        ["opt2"]  = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000002"), Name = "Nero Zaffiro",           Description = "Nero metallizzato profondo",                             Category = OptionCategory.Color,      Price = 1200, Color = "#1a1a1a" },
        ["opt3"]  = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000003"), Name = "Blu Portimao",           Description = "Blu metallizzato intenso",                               Category = OptionCategory.Color,      Price = 1200, Color = "#1E3A5F" },
        ["opt4"]  = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000004"), Name = "Rosso Aventurin",        Description = "Rosso metallizzato vibrante",                            Category = OptionCategory.Color,      Price = 1500, Color = "#8B1A1A" },
        ["opt5"]  = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000005"), Name = "Grigio Mineral",         Description = "Grigio opaco effetto pietra",                            Category = OptionCategory.Color,      Price = 1800, Color = "#708090" },
        ["opt6"]  = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000006"), Name = "Interni Standard Nero",  Description = "Interni in tessuto nero con cuciture a contrasto",        Category = OptionCategory.Interior,   Price = 0 },
        ["opt7"]  = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000007"), Name = "Interni Dakota Beige",   Description = "Sedili in pelle Dakota color beige",                      Category = OptionCategory.Interior,   Price = 2800 },
        ["opt8"]  = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000008"), Name = "Interni Merino Cognac",  Description = "Pelle Merino pieno fiore color cognac",                   Category = OptionCategory.Interior,   Price = 5200 },
        ["opt9"]  = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000009"), Name = "Sistema di Navigazione", Description = "GPS integrato con aggiornamenti mappe",                   Category = OptionCategory.Technology, Price = 1800 },
        ["opt10"] = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000010"), Name = "Pacchetto Parcheggio",   Description = "Sensori parcheggio anteriori e posteriori + telecamera",  Category = OptionCategory.Technology, Price = 900 },
        ["opt11"] = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000011"), Name = "Head-Up Display",        Description = "Proiezione velocità e navigazione sul parabrezza",         Category = OptionCategory.Technology, Price = 1200 },
        ["opt12"] = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000012"), Name = "Illuminazione Ambiente", Description = "Illuminazione ambientale interna a 64 colori",             Category = OptionCategory.Technology, Price = 500 },
        ["opt13"] = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000013"), Name = "Harman Kardon Audio",    Description = "Impianto audio premium 17 altoparlanti",                  Category = OptionCategory.Technology, Price = 2200 },
        ["opt14"] = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000014"), Name = "Pacchetto Driver Assist",Description = "ACC, Lane Keeping, Emergency Brake",                      Category = OptionCategory.Safety,     Price = 2400 },
        ["opt15"] = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000015"), Name = "Monitoraggio Angolo Cieco", Description = "Rilevamento veicoli negli angoli ciechi",              Category = OptionCategory.Safety,     Price = 800 },
        ["opt16"] = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000016"), Name = "Night Vision",           Description = "Sistema visione notturna con rilevamento pedoni",          Category = OptionCategory.Safety,     Price = 2100 },
        ["opt17"] = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000017"), Name = "Sedili Riscaldati",      Description = "Riscaldamento anteriore e posteriore",                    Category = OptionCategory.Comfort,    Price = 600 },
        ["opt18"] = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000018"), Name = "Sedili Ventilati",       Description = "Ventilazione sedili anteriori",                           Category = OptionCategory.Comfort,    Price = 900 },
        ["opt19"] = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000019"), Name = "Tetto Panoramico",       Description = "Tetto apribile in vetro panoramico",                      Category = OptionCategory.Comfort,    Price = 1900 },
        ["opt20"] = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000020"), Name = "Keyless Entry",          Description = "Accesso e avviamento senza chiave",                       Category = OptionCategory.Comfort,    Price = 700 },
        ["opt21"] = new() { Id = Guid.Parse("33333333-0000-0000-0000-000000000021"), Name = "Portellone Elettrico",   Description = "Apertura/chiusura elettrica del bagagliaio",              Category = OptionCategory.Comfort,    Price = 1100 },
    };

    private static async Task SeedIncompatibilitiesAsync(AppDbContext db, Dictionary<string, CarOption> opts)
    {
        // Colors are mutually exclusive
        var colors = new[] { "opt1", "opt2", "opt3", "opt4", "opt5" };
        foreach (var a in colors)
            foreach (var b in colors.Where(x => x != a))
            {
                var optA = await db.CarOptions.Include(o => o.IncompatibleWith).FirstAsync(o => o.Id == opts[a].Id);
                var optB = await db.CarOptions.FirstAsync(o => o.Id == opts[b].Id);
                if (!optA.IncompatibleWith.Contains(optB))
                    optA.IncompatibleWith.Add(optB);
            }

        // Interiors are mutually exclusive
        var interiors = new[] { "opt6", "opt7", "opt8" };
        foreach (var a in interiors)
            foreach (var b in interiors.Where(x => x != a))
            {
                var optA = await db.CarOptions.Include(o => o.IncompatibleWith).FirstAsync(o => o.Id == opts[a].Id);
                var optB = await db.CarOptions.FirstAsync(o => o.Id == opts[b].Id);
                if (!optA.IncompatibleWith.Contains(optB))
                    optA.IncompatibleWith.Add(optB);
            }

        await db.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AppDbContext db)
    {
        await db.Users.AddRangeAsync(
            new User { Id = Guid.Parse("44444444-0000-0000-0000-000000000001"), Email = "admin@autoconfig.it", Name = "Admin Sistema",   PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),  Role = Role.Admin },
            new User { Id = Guid.Parse("44444444-0000-0000-0000-000000000002"), Email = "mario@example.com",   Name = "Mario Rossi",     PasswordHash = BCrypt.Net.BCrypt.HashPassword("mario123"),  Role = Role.User },
            new User { Id = Guid.Parse("44444444-0000-0000-0000-000000000003"), Email = "giulia@example.com",  Name = "Giulia Bianchi",  PasswordHash = BCrypt.Net.BCrypt.HashPassword("giulia123"), Role = Role.User }
        );
        await db.SaveChangesAsync();
    }
}
