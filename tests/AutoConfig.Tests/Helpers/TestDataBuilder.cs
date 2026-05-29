using AutoConfig.Core.Entities;
using AutoConfig.Core.Enums;
using AutoConfig.Infrastructure.Data;
using BCrypt.Net;

namespace AutoConfig.Tests.Helpers;

public static class TestDataBuilder
{
    public static User AdminUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "admin@test.it",
        Name = "Test Admin",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
        Role = Role.Admin
    };

    public static User RegularUser(string? email = null) => new()
    {
        Id = Guid.NewGuid(),
        Email = email ?? "user@test.it",
        Name = "Test User",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
        Role = Role.User
    };

    public static CarModel CarModel(decimal basePrice = 40000m) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Model",
        Brand = "Test Brand",
        Category = CarCategory.Sedan,
        BasePrice = basePrice,
        Description = "Test description",
        ImageColor = "#000000"
    };

    public static Motorization Motorization(Guid modelId, decimal price = 0m) => new()
    {
        Id = Guid.NewGuid(),
        ModelId = modelId,
        Name = "Test Engine",
        FuelType = FuelType.Petrol,
        Power = 150,
        Torque = 300,
        Acceleration = 8.0m,
        Consumption = "6.0 L/100km",
        Price = price
    };

    public static CarOption Option(OptionCategory category = OptionCategory.Technology, decimal price = 500m) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Option",
        Description = "Test option description",
        Category = category,
        Price = price
    };

    public static VehicleConfiguration Configuration(Guid userId, Guid modelId, Guid motorizationId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = "Test Config",
        ModelId = modelId,
        MotorizationId = motorizationId,
        TotalPrice = 45000m
    };

    public static Quote Quote(Guid userId, Guid configId, decimal price = 45000m) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        ConfigurationId = configId,
        TotalPrice = price,
        Discount = 0,
        FinalPrice = price,
        ExpiresAt = DateTime.UtcNow.AddDays(90)
    };

    public static async Task<(User user, CarModel model, Motorization motorization, VehicleConfiguration config)>
        SeedBasicConfigAsync(AppDbContext db)
    {
        var user = RegularUser();
        var model = CarModel();
        var mot = Motorization(model.Id, 2000m);
        var config = Configuration(user.Id, model.Id, mot.Id);
        config.TotalPrice = model.BasePrice + mot.Price;

        db.Users.Add(user);
        db.CarModels.Add(model);
        db.Motorizations.Add(mot);
        db.Configurations.Add(config);
        await db.SaveChangesAsync();

        return (user, model, mot, config);
    }
}
