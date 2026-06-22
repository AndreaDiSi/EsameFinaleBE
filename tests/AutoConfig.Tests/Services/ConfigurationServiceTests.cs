using AutoConfig.Core.Entities;
using AutoConfig.Core.Exceptions;
using AutoConfig.Core.Interfaces;
using AutoConfig.Infrastructure.Data;
using AutoConfig.Infrastructure.Repositories;
using AutoConfig.Infrastructure.Services;
using AutoConfig.Tests.Helpers;
using FluentAssertions;

namespace AutoConfig.Tests.Services;

public class ConfigurationServiceTests
{
    private AppDbContext _db = null!;
    private ConfigurationService _sut = null!;

    private void Setup()
    {
        _db = TestDbContextFactory.Create();
        _sut = new ConfigurationService(
            new ConfigurationRepository(_db),
            new CarModelRepository(_db),
            new CarOptionRepository(_db));
    }

    [Fact]
    public async Task Create_ValidData_ReturnsConfiguration()
    {
        Setup();
        var user = TestDataBuilder.RegularUser();
        var model = TestDataBuilder.CarModel(30000m);
        var mot = TestDataBuilder.Motorization(model.Id, 5000m);
        _db.Users.Add(user);
        _db.CarModels.Add(model);
        _db.Motorizations.Add(mot);
        await _db.SaveChangesAsync();

        var config = await _sut.CreateAsync(user.Id, "My Config", model.Id, mot.Id, []);

        config.Name.Should().Be("My Config");
        config.UserId.Should().Be(user.Id);
        config.TotalPrice.Should().Be(35000m);
    }

    [Fact]
    public async Task Create_InvalidModel_ThrowsNotFoundException()
    {
        Setup();
        await _sut.Invoking(s => s.CreateAsync(Guid.NewGuid(), "X", Guid.NewGuid(), Guid.NewGuid(), []))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_MotorizationNotBelongingToModel_ThrowsValidationException()
    {
        Setup();
        var model1 = TestDataBuilder.CarModel();
        var model2 = TestDataBuilder.CarModel();
        var mot = TestDataBuilder.Motorization(model2.Id);
        _db.CarModels.AddRange(model1, model2);
        _db.Motorizations.Add(mot);
        await _db.SaveChangesAsync();

        await _sut.Invoking(s => s.CreateAsync(Guid.NewGuid(), "X", model1.Id, mot.Id, []))
            .Should().ThrowAsync<Core.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task Create_IncompatibleOptions_ThrowsValidationException()
    {
        Setup();
        var model = TestDataBuilder.CarModel();
        var mot = TestDataBuilder.Motorization(model.Id);
        var opt1 = TestDataBuilder.Option(Core.Enums.OptionCategory.Color);
        var opt2 = TestDataBuilder.Option(Core.Enums.OptionCategory.Color);
        opt1.IncompatibleWith.Add(opt2);
        _db.CarModels.Add(model);
        _db.Motorizations.Add(mot);
        _db.CarOptions.AddRange(opt1, opt2);
        await _db.SaveChangesAsync();

        await _sut.Invoking(s => s.CreateAsync(Guid.NewGuid(), "X", model.Id, mot.Id, [opt1.Id, opt2.Id]))
            .Should().ThrowAsync<Core.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task Create_CalculatesCorrectTotalPrice()
    {
        Setup();
        var model = TestDataBuilder.CarModel(20000m);
        var mot = TestDataBuilder.Motorization(model.Id, 3000m);
        var opt1 = TestDataBuilder.Option(price: 1000m);
        var opt2 = TestDataBuilder.Option(price: 500m);
        _db.CarModels.Add(model);
        _db.Motorizations.Add(mot);
        _db.CarOptions.AddRange(opt1, opt2);
        var user = TestDataBuilder.RegularUser();
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var config = await _sut.CreateAsync(user.Id, "Price Test", model.Id, mot.Id, [opt1.Id, opt2.Id]);

        config.TotalPrice.Should().Be(24500m);
    }

    [Fact]
    public async Task Update_NotOwner_ThrowsForbiddenException()
    {
        Setup();
        var (_, model, mot, config) = await TestDataBuilder.SeedBasicConfigAsync(_db);
        var otherUserId = Guid.NewGuid();

        await _sut.Invoking(s => s.UpdateAsync(config.Id, otherUserId, false, new UpdateConfigurationCommand("Hacked", model.Id, mot.Id, [])))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Update_AdminCanUpdateAnyConfig()
    {
        Setup();
        var (_, model, mot, config) = await TestDataBuilder.SeedBasicConfigAsync(_db);

        var updated = await _sut.UpdateAsync(config.Id, Guid.NewGuid(), isAdmin: true, new UpdateConfigurationCommand("Updated", model.Id, mot.Id, []));

        updated.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task Delete_NotOwner_ThrowsForbiddenException()
    {
        Setup();
        var (_, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(_db);

        await _sut.Invoking(s => s.DeleteAsync(config.Id, Guid.NewGuid(), false))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Delete_Owner_Succeeds()
    {
        Setup();
        var (user, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(_db);

        await _sut.DeleteAsync(config.Id, user.Id, false);

        _db.Configurations.Find(config.Id).Should().BeNull();
    }

    [Fact]
    public async Task GetUserConfigurations_ReturnsOnlyUserConfigs()
    {
        Setup();
        var (user, model, mot, _) = await TestDataBuilder.SeedBasicConfigAsync(_db);
        var otherUser = TestDataBuilder.RegularUser("other@test.it");
        var otherConfig = TestDataBuilder.Configuration(otherUser.Id, model.Id, mot.Id);
        _db.Users.Add(otherUser);
        _db.Configurations.Add(otherConfig);
        await _db.SaveChangesAsync();

        var configs = await _sut.GetUserConfigurationsAsync(user.Id);

        configs.Should().HaveCount(1);
        configs[0].UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Create_NonExistentOptions_ThrowsValidationException()
    {
        Setup();
        var model = TestDataBuilder.CarModel();
        var mot = TestDataBuilder.Motorization(model.Id);
        _db.CarModels.Add(model);
        _db.Motorizations.Add(mot);
        await _db.SaveChangesAsync();

        await _sut.Invoking(s => s.CreateAsync(Guid.NewGuid(), "X", model.Id, mot.Id, [Guid.NewGuid()]))
            .Should().ThrowAsync<Core.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task Create_OptionRequiresOtherMotorization_ThrowsValidationException()
    {
        Setup();
        var model = TestDataBuilder.CarModel();
        var mot1 = TestDataBuilder.Motorization(model.Id);
        var mot2 = TestDataBuilder.Motorization(model.Id);
        var opt = TestDataBuilder.Option();
        opt.RequiredMotorizations.Add(mot2);
        _db.CarModels.Add(model);
        _db.Motorizations.AddRange(mot1, mot2);
        _db.CarOptions.Add(opt);
        await _db.SaveChangesAsync();

        await _sut.Invoking(s => s.CreateAsync(Guid.NewGuid(), "X", model.Id, mot1.Id, [opt.Id]))
            .Should().ThrowAsync<Core.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task Update_Owner_UpdatesNameAndRecalculatesPrice()
    {
        Setup();
        var (user, model, mot, config) = await TestDataBuilder.SeedBasicConfigAsync(_db);
        var opt = TestDataBuilder.Option(price: 1500m);
        _db.CarOptions.Add(opt);
        await _db.SaveChangesAsync();

        var updated = await _sut.UpdateAsync(config.Id, user.Id, false,
            new UpdateConfigurationCommand("Nuovo Nome", model.Id, mot.Id, [opt.Id]));

        updated.Name.Should().Be("Nuovo Nome");
        updated.TotalPrice.Should().Be(model.BasePrice + mot.Price + 1500m);
    }

    [Fact]
    public async Task Get_Owner_ReturnsConfiguration()
    {
        Setup();
        var (user, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(_db);

        var result = await _sut.GetAsync(config.Id, user.Id, isAdmin: false);

        result.Id.Should().Be(config.Id);
    }

    [Fact]
    public async Task Get_Admin_CanAccessAnyConfiguration()
    {
        Setup();
        var (_, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(_db);

        var result = await _sut.GetAsync(config.Id, Guid.NewGuid(), isAdmin: true);

        result.Id.Should().Be(config.Id);
    }

    [Fact]
    public async Task Get_NonExistent_ThrowsNotFoundException()
    {
        Setup();

        await _sut.Invoking(s => s.GetAsync(Guid.NewGuid(), Guid.NewGuid(), false))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_Admin_CanDeleteAnyConfiguration()
    {
        Setup();
        var (_, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(_db);

        await _sut.DeleteAsync(config.Id, Guid.NewGuid(), isAdmin: true);

        _db.Configurations.Find(config.Id).Should().BeNull();
    }
}
