using System.Net;
using System.Net.Http.Json;
using AutoConfig.Api.DTOs.Auth;
using AutoConfig.Api.DTOs.Catalog;
using FluentAssertions;

namespace AutoConfig.Tests.Integration;

public class CatalogAdminIntegrationTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;

    public CatalogAdminIntegrationTests(WebAppFactory factory) => _factory = factory;

    private async Task<HttpClient> AdminClientAsync()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = "admin@autoconfig.it", Password = "admin123" });
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new("Bearer", auth!.Token);
        return client;
    }

    private async Task<HttpClient> UserClientAsync()
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = "mario@example.com", Password = "mario123" });
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new("Bearer", auth!.Token);
        return client;
    }

    private static object NewModelPayload(string name = "Test Model") => new
    {
        Name = name,
        Brand = "Test Brand",
        Category = "Sedan",
        BasePrice = 35000m,
        Description = "Integration test model",
        ImageColor = "#FF0000"
    };

    // ── CreateModel ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateModel_AsAdmin_Returns201WithCarModelDto()
    {
        var client = await AdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/catalog/models", NewModelPayload("Admin Test Model"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var model = await response.Content.ReadFromJsonAsync<CarModelDto>();
        model.Should().NotBeNull();
        model!.Name.Should().Be("Admin Test Model");
        model.Brand.Should().Be("Test Brand");
    }

    [Fact]
    public async Task CreateModel_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/catalog/models", NewModelPayload());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateModel_InvalidCategory_Returns422()
    {
        var client = await AdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/catalog/models", new
        {
            Name = "Bad Cat Model", Brand = "Test", Category = "flying_saucer",
            BasePrice = 10000m, Description = "X", ImageColor = "#000"
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── UpdateModel ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateModel_AsAdmin_Returns200WithUpdatedData()
    {
        var client = await AdminClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/catalog/models", NewModelPayload("Update Source")))
            .Content.ReadFromJsonAsync<CarModelDto>();

        var response = await client.PutAsJsonAsync($"/api/catalog/models/{created!.Id}", new
        {
            Name = "Updated Name", Brand = "Updated Brand", Category = "Suv",
            BasePrice = 50000m, Description = "Updated desc", ImageColor = "#00FF00"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CarModelDto>();
        updated!.Name.Should().Be("Updated Name");
        updated.Brand.Should().Be("Updated Brand");
        updated.Category.Should().Be("Suv");
        updated.BasePrice.Should().Be(50000m);
    }

    [Fact]
    public async Task UpdateModel_AsRegularUser_Returns403()
    {
        var admin = await AdminClientAsync();
        var created = await (await admin.PostAsJsonAsync("/api/catalog/models", NewModelPayload("User Cannot Update")))
            .Content.ReadFromJsonAsync<CarModelDto>();

        var user = await UserClientAsync();
        var response = await user.PutAsJsonAsync($"/api/catalog/models/{created!.Id}", NewModelPayload());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateModel_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/catalog/models/{Guid.NewGuid()}", NewModelPayload());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateModel_NonExistentModel_Returns404()
    {
        var client = await AdminClientAsync();

        var response = await client.PutAsJsonAsync($"/api/catalog/models/{Guid.NewGuid()}", NewModelPayload());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DeleteModel ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteModel_AsAdmin_Returns204()
    {
        var client = await AdminClientAsync();
        var created = await (await client.PostAsJsonAsync("/api/catalog/models", NewModelPayload("To Delete")))
            .Content.ReadFromJsonAsync<CarModelDto>();

        var response = await client.DeleteAsync($"/api/catalog/models/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteModel_AsRegularUser_Returns403()
    {
        var admin = await AdminClientAsync();
        var created = await (await admin.PostAsJsonAsync("/api/catalog/models", NewModelPayload("User Cannot Delete")))
            .Content.ReadFromJsonAsync<CarModelDto>();

        var user = await UserClientAsync();
        var response = await user.DeleteAsync($"/api/catalog/models/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteModel_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/catalog/models/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteModel_NonExistentModel_Returns404()
    {
        var client = await AdminClientAsync();

        var response = await client.DeleteAsync($"/api/catalog/models/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── CreateMotorization ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateMotorization_AsAdmin_Returns201WithMotorizationDto()
    {
        var client = await AdminClientAsync();

        var modelResp = await client.PostAsJsonAsync("/api/catalog/models", NewModelPayload("Motorization Test Model"));
        var model = await modelResp.Content.ReadFromJsonAsync<CarModelDto>();

        var response = await client.PostAsJsonAsync(
            $"/api/catalog/models/{model!.Id}/motorizations", new
            {
                Name = "Test Engine 2.0T",
                FuelType = "Petrol",
                Power = 200,
                Torque = 350,
                Acceleration = 7.5m,
                Consumption = "7.0 L/100km",
                Price = 5000m
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var mot = await response.Content.ReadFromJsonAsync<MotorizationDto>();
        mot.Should().NotBeNull();
        mot!.Name.Should().Be("Test Engine 2.0T");
        mot.Power.Should().Be(200);
    }

    [Fact]
    public async Task CreateMotorization_InvalidFuelType_Returns422()
    {
        var client = await AdminClientAsync();
        var model = await (await client.PostAsJsonAsync("/api/catalog/models", NewModelPayload("FuelType Test Model")))
            .Content.ReadFromJsonAsync<CarModelDto>();

        var response = await client.PostAsJsonAsync(
            $"/api/catalog/models/{model!.Id}/motorizations", new
            {
                Name = "Bad Engine", FuelType = "steam",
                Power = 100, Torque = 200, Acceleration = 10m,
                Consumption = "5.0 L/100km", Price = 0m
            });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateMotorization_NonExistentModel_Returns404()
    {
        var client = await AdminClientAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/catalog/models/{Guid.NewGuid()}/motorizations", new
            {
                Name = "Orphan Engine", FuelType = "Petrol",
                Power = 150, Torque = 300, Acceleration = 8m,
                Consumption = "6.0 L/100km", Price = 0m
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── UpdateMotorization ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateMotorization_AsAdmin_Returns200WithAllFieldsUpdated()
    {
        var client = await AdminClientAsync();
        var model = await CreateModelAsync(client, "UpdateMot Model");
        var mot = await CreateMotorizationAsync(client, model.Id, "Original Engine");

        var response = await client.PutAsJsonAsync(
            $"/api/catalog/models/{model.Id}/motorizations/{mot.Id}", new
            {
                Name = "Updated Engine 3.0T",
                FuelType = "Diesel",
                Power = 300,
                Torque = 600,
                Acceleration = 5.2m,
                Consumption = "8.5 L/100km",
                Price = 9000m
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<MotorizationDto>();
        updated!.Name.Should().Be("Updated Engine 3.0T");
        updated.FuelType.Should().Be("Diesel");
        updated.Power.Should().Be(300);
        updated.Torque.Should().Be(600);
        updated.Acceleration.Should().Be(5.2m);
        updated.Price.Should().Be(9000m);
        updated.ModelId.Should().Be(model.Id);
    }

    [Fact]
    public async Task UpdateMotorization_InvalidFuelType_Returns422()
    {
        var client = await AdminClientAsync();
        var model = await CreateModelAsync(client, "UpdateMot FuelType Model");
        var mot = await CreateMotorizationAsync(client, model.Id, "Engine FuelType");

        var response = await client.PutAsJsonAsync(
            $"/api/catalog/models/{model.Id}/motorizations/{mot.Id}", new
            {
                Name = "Bad", FuelType = "nuclear",
                Power = 100, Torque = 200, Acceleration = 10m,
                Consumption = "5.0 L/100km", Price = 0m
            });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateMotorization_NonExistentModel_Returns404()
    {
        var client = await AdminClientAsync();

        var response = await client.PutAsJsonAsync(
            $"/api/catalog/models/{Guid.NewGuid()}/motorizations/{Guid.NewGuid()}", new
            {
                Name = "X", FuelType = "Petrol",
                Power = 100, Torque = 200, Acceleration = 10m,
                Consumption = "5.0 L/100km", Price = 0m
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateMotorization_NonExistentMotorization_Returns404()
    {
        var client = await AdminClientAsync();
        var model = await CreateModelAsync(client, "UpdateMot NoMot Model");

        var response = await client.PutAsJsonAsync(
            $"/api/catalog/models/{model.Id}/motorizations/{Guid.NewGuid()}", new
            {
                Name = "Ghost", FuelType = "Petrol",
                Power = 100, Torque = 200, Acceleration = 10m,
                Consumption = "5.0 L/100km", Price = 0m
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateMotorization_MotorizationBelongsToOtherModel_Returns404()
    {
        var client = await AdminClientAsync();
        var modelA = await CreateModelAsync(client, "UpdateMot ModelA");
        var modelB = await CreateModelAsync(client, "UpdateMot ModelB");
        var motOfB = await CreateMotorizationAsync(client, modelB.Id, "Engine of B");

        // Try to update motOfB using modelA's route — should fail
        var response = await client.PutAsJsonAsync(
            $"/api/catalog/models/{modelA.Id}/motorizations/{motOfB.Id}", new
            {
                Name = "Hijack", FuelType = "Petrol",
                Power = 100, Torque = 200, Acceleration = 10m,
                Consumption = "5.0 L/100km", Price = 0m
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateMotorization_AsRegularUser_Returns403()
    {
        var admin = await AdminClientAsync();
        var model = await CreateModelAsync(admin, "UpdateMot User Model");
        var mot = await CreateMotorizationAsync(admin, model.Id, "Engine User");

        var user = await UserClientAsync();
        var response = await user.PutAsJsonAsync(
            $"/api/catalog/models/{model.Id}/motorizations/{mot.Id}", new
            {
                Name = "Hijack", FuelType = "Petrol",
                Power = 100, Torque = 200, Acceleration = 10m,
                Consumption = "5.0 L/100km", Price = 0m
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateMotorization_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/api/catalog/models/{Guid.NewGuid()}/motorizations/{Guid.NewGuid()}", new
            {
                Name = "X", FuelType = "Petrol",
                Power = 100, Torque = 200, Acceleration = 10m,
                Consumption = "5.0 L/100km", Price = 0m
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── DeleteMotorization ───────────────────────────────────────────────────

    [Fact]
    public async Task DeleteMotorization_AsAdmin_Returns204()
    {
        var client = await AdminClientAsync();
        var model = await CreateModelAsync(client, "DeleteMot Model");
        var mot = await CreateMotorizationAsync(client, model.Id, "Engine To Delete");

        var response = await client.DeleteAsync(
            $"/api/catalog/models/{model.Id}/motorizations/{mot.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteMotorization_NonExistentMotorization_Returns404()
    {
        var client = await AdminClientAsync();
        var model = await CreateModelAsync(client, "DeleteMot NoMot Model");

        var response = await client.DeleteAsync(
            $"/api/catalog/models/{model.Id}/motorizations/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteMotorization_MotorizationBelongsToOtherModel_Returns404()
    {
        var client = await AdminClientAsync();
        var modelA = await CreateModelAsync(client, "DeleteMot ModelA");
        var modelB = await CreateModelAsync(client, "DeleteMot ModelB");
        var motOfB = await CreateMotorizationAsync(client, modelB.Id, "Engine of B Del");

        // Try to delete motOfB using modelA's route — should fail
        var response = await client.DeleteAsync(
            $"/api/catalog/models/{modelA.Id}/motorizations/{motOfB.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteMotorization_AsRegularUser_Returns403()
    {
        var admin = await AdminClientAsync();
        var model = await CreateModelAsync(admin, "DeleteMot User Model");
        var mot = await CreateMotorizationAsync(admin, model.Id, "Engine No Delete");

        var user = await UserClientAsync();
        var response = await user.DeleteAsync(
            $"/api/catalog/models/{model.Id}/motorizations/{mot.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteMotorization_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.DeleteAsync(
            $"/api/catalog/models/{Guid.NewGuid()}/motorizations/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── CreateOption ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOption_AsAdmin_Returns201WithCorrectData()
    {
        var client = await AdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/catalog/options", new
        {
            Name = "Tetto Panoramico",
            Description = "Tetto apribile in vetro",
            Category = "Comfort",
            Price = 1500m,
            Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var option = await response.Content.ReadFromJsonAsync<CarOptionDto>();
        option!.Name.Should().Be("Tetto Panoramico");
        option.Category.Should().Be("Comfort");
        option.Price.Should().Be(1500m);
        option.IncompatibleWith.Should().BeEmpty();
        option.RequiredMotorizations.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateOption_WithIncompatibilities_StoresRelations()
    {
        var client = await AdminClientAsync();

        // Create two existing options to be incompatible with
        var optB = await CreateOptionAsync(client, "Incompatible B");
        var optC = await CreateOptionAsync(client, "Incompatible C");

        var response = await client.PostAsJsonAsync("/api/catalog/options", new
        {
            Name = "Option With Incompatibilities",
            Description = "desc",
            Category = "Color",
            Price = 800m,
            Color = "#FF0000",
            IncompatibleWith = new[] { optB.Id, optC.Id },
            RequiredMotorizationIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var option = await response.Content.ReadFromJsonAsync<CarOptionDto>();
        option!.IncompatibleWith.Should().HaveCount(2)
            .And.Contain(optB.Id)
            .And.Contain(optC.Id);
    }

    [Fact]
    public async Task CreateOption_WithRequiredMotorizations_StoresRelations()
    {
        var client = await AdminClientAsync();
        var model = await CreateModelAsync(client, "CreateOpt Mot Model");
        var mot = await CreateMotorizationAsync(client, model.Id, "Required Engine");

        var response = await client.PostAsJsonAsync("/api/catalog/options", new
        {
            Name = "Option With Required Mot",
            Description = "desc",
            Category = "Technology",
            Price = 2000m,
            Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = new[] { mot.Id }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var option = await response.Content.ReadFromJsonAsync<CarOptionDto>();
        option!.RequiredMotorizations.Should().HaveCount(1)
            .And.Contain(mot.Id);
    }

    [Fact]
    public async Task CreateOption_InvalidCategory_Returns422()
    {
        var client = await AdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/catalog/options", new
        {
            Name = "Bad Category Option",
            Description = "desc",
            Category = "flying_saucer",
            Price = 100m,
            Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateOption_AsRegularUser_Returns403()
    {
        var user = await UserClientAsync();

        var response = await user.PostAsJsonAsync("/api/catalog/options", new
        {
            Name = "User Option",
            Description = "desc",
            Category = "Comfort",
            Price = 100m,
            Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateOption_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/catalog/options", new
        {
            Name = "Anon Option",
            Description = "desc",
            Category = "Comfort",
            Price = 100m,
            Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── UpdateOption ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateOption_AsAdmin_Returns200WithUpdatedFields()
    {
        var client = await AdminClientAsync();
        var opt = await CreateOptionAsync(client, "Original Option Name");

        var response = await client.PutAsJsonAsync($"/api/catalog/options/{opt.Id}", new
        {
            Name = "Updated Option Name",
            Description = "Updated description",
            Category = "Safety",
            Price = 3000m,
            Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<CarOptionDto>();
        updated!.Name.Should().Be("Updated Option Name");
        updated.Description.Should().Be("Updated description");
        updated.Category.Should().Be("Safety");
        updated.Price.Should().Be(3000m);
    }

    [Fact]
    public async Task UpdateOption_ReplacesIncompatibilities()
    {
        var client = await AdminClientAsync();
        var optB = await CreateOptionAsync(client, "Inc Replace B");
        var optC = await CreateOptionAsync(client, "Inc Replace C");

        // Create optA incompatible with optB
        var createResp = await client.PostAsJsonAsync("/api/catalog/options", new
        {
            Name = "Option Inc Replace A",
            Description = "desc",
            Category = "Color",
            Price = 100m,
            Color = "#FF0000",
            IncompatibleWith = new[] { optB.Id },
            RequiredMotorizationIds = Array.Empty<Guid>()
        });
        var optA = await createResp.Content.ReadFromJsonAsync<CarOptionDto>();
        optA!.IncompatibleWith.Should().Contain(optB.Id);

        // Update optA to be incompatible with optC instead
        var updateResp = await client.PutAsJsonAsync($"/api/catalog/options/{optA.Id}", new
        {
            Name = "Option Inc Replace A",
            Description = "desc",
            Category = "Color",
            Price = 100m,
            Color = "#FF0000",
            IncompatibleWith = new[] { optC.Id },
            RequiredMotorizationIds = Array.Empty<Guid>()
        });

        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResp.Content.ReadFromJsonAsync<CarOptionDto>();
        updated!.IncompatibleWith.Should().ContainSingle()
            .Which.Should().Be(optC.Id);
        updated.IncompatibleWith.Should().NotContain(optB.Id);
    }

    [Fact]
    public async Task UpdateOption_ReplacesRequiredMotorizations()
    {
        var client = await AdminClientAsync();
        var model = await CreateModelAsync(client, "UpdateOpt Mot Model");
        var mot1 = await CreateMotorizationAsync(client, model.Id, "Mot1 Replace");
        var mot2 = await CreateMotorizationAsync(client, model.Id, "Mot2 Replace");

        // Create option requiring mot1
        var createResp = await client.PostAsJsonAsync("/api/catalog/options", new
        {
            Name = "Option Mot Replace",
            Description = "desc",
            Category = "Technology",
            Price = 500m,
            Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = new[] { mot1.Id }
        });
        var opt = await createResp.Content.ReadFromJsonAsync<CarOptionDto>();
        opt!.RequiredMotorizations.Should().Contain(mot1.Id);

        // Update option to require mot2 instead
        var updateResp = await client.PutAsJsonAsync($"/api/catalog/options/{opt.Id}", new
        {
            Name = "Option Mot Replace",
            Description = "desc",
            Category = "Technology",
            Price = 500m,
            Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = new[] { mot2.Id }
        });

        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResp.Content.ReadFromJsonAsync<CarOptionDto>();
        updated!.RequiredMotorizations.Should().ContainSingle()
            .Which.Should().Be(mot2.Id);
        updated.RequiredMotorizations.Should().NotContain(mot1.Id);
    }

    [Fact]
    public async Task UpdateOption_ClearsRelationsWhenEmptyListProvided()
    {
        var client = await AdminClientAsync();
        var model = await CreateModelAsync(client, "UpdateOpt Clear Model");
        var mot = await CreateMotorizationAsync(client, model.Id, "Mot Clear");
        var otherOpt = await CreateOptionAsync(client, "Inc Clear Other");

        // Create option with relations
        var createResp = await client.PostAsJsonAsync("/api/catalog/options", new
        {
            Name = "Option To Clear",
            Description = "desc",
            Category = "Interior",
            Price = 600m,
            Color = (string?)null,
            IncompatibleWith = new[] { otherOpt.Id },
            RequiredMotorizationIds = new[] { mot.Id }
        });
        var opt = await createResp.Content.ReadFromJsonAsync<CarOptionDto>();
        opt!.IncompatibleWith.Should().NotBeEmpty();
        opt.RequiredMotorizations.Should().NotBeEmpty();

        // Update with empty lists to clear all relations
        var updateResp = await client.PutAsJsonAsync($"/api/catalog/options/{opt.Id}", new
        {
            Name = "Option To Clear",
            Description = "desc",
            Category = "Interior",
            Price = 600m,
            Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = Array.Empty<Guid>()
        });

        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResp.Content.ReadFromJsonAsync<CarOptionDto>();
        updated!.IncompatibleWith.Should().BeEmpty();
        updated.RequiredMotorizations.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateOption_InvalidCategory_Returns422()
    {
        var client = await AdminClientAsync();
        var opt = await CreateOptionAsync(client, "UpdateOpt BadCat");

        var response = await client.PutAsJsonAsync($"/api/catalog/options/{opt.Id}", new
        {
            Name = "ValidName", Description = "desc",
            Category = "not_a_category",
            Price = 100m, Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateOption_NonExistentOption_Returns404()
    {
        var client = await AdminClientAsync();

        var response = await client.PutAsJsonAsync($"/api/catalog/options/{Guid.NewGuid()}", new
        {
            Name = "Ghost", Description = "desc",
            Category = "Comfort",
            Price = 100m, Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateOption_AsRegularUser_Returns403()
    {
        var admin = await AdminClientAsync();
        var opt = await CreateOptionAsync(admin, "UpdateOpt User Opt");

        var user = await UserClientAsync();
        var response = await user.PutAsJsonAsync($"/api/catalog/options/{opt.Id}", new
        {
            Name = "Hijack", Description = "desc",
            Category = "Comfort",
            Price = 100m, Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── DeleteOption ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteOption_AsAdmin_Returns204()
    {
        var client = await AdminClientAsync();
        var opt = await CreateOptionAsync(client, "DeleteOpt Option");

        var response = await client.DeleteAsync($"/api/catalog/options/{opt.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteOption_AsRegularUser_Returns403()
    {
        var admin = await AdminClientAsync();
        var opt = await CreateOptionAsync(admin, "DeleteOpt User Opt");

        var user = await UserClientAsync();
        var response = await user.DeleteAsync($"/api/catalog/options/{opt.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteOption_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/catalog/options/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteOption_NonExistentOption_Returns404()
    {
        var client = await AdminClientAsync();

        var response = await client.DeleteAsync($"/api/catalog/options/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<CarModelDto> CreateModelAsync(HttpClient client, string name)
    {
        var resp = await client.PostAsJsonAsync("/api/catalog/models", NewModelPayload(name));
        return (await resp.Content.ReadFromJsonAsync<CarModelDto>())!;
    }

    private static async Task<MotorizationDto> CreateMotorizationAsync(HttpClient client, Guid modelId, string name)
    {
        var resp = await client.PostAsJsonAsync($"/api/catalog/models/{modelId}/motorizations", new
        {
            Name = name,
            FuelType = "Petrol",
            Power = 150,
            Torque = 300,
            Acceleration = 8.0m,
            Consumption = "6.0 L/100km",
            Price = 3000m
        });
        return (await resp.Content.ReadFromJsonAsync<MotorizationDto>())!;
    }

    private static async Task<CarOptionDto> CreateOptionAsync(HttpClient client, string name)
    {
        var resp = await client.PostAsJsonAsync("/api/catalog/options", new
        {
            Name = name,
            Description = "Helper option",
            Category = "Technology",
            Price = 500m,
            Color = (string?)null,
            IncompatibleWith = Array.Empty<Guid>(),
            RequiredMotorizationIds = Array.Empty<Guid>()
        });
        return (await resp.Content.ReadFromJsonAsync<CarOptionDto>())!;
    }
}
