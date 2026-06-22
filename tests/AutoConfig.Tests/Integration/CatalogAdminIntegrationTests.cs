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
}
