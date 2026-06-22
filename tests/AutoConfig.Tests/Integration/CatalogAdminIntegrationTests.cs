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

    [Fact]
    public async Task CreateModel_AsAdmin_Returns201WithCarModelDto()
    {
        var client = await AdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/catalog/models", new
        {
            Name = "Admin Test Model",
            Brand = "Test Brand",
            Category = "Sedan",
            BasePrice = 35000m,
            Description = "Integration test model",
            ImageColor = "#FF0000"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var model = await response.Content.ReadFromJsonAsync<CarModelDto>();
        model.Should().NotBeNull();
        model!.Name.Should().Be("Admin Test Model");
        model.Brand.Should().Be("Test Brand");
    }

    [Fact]
    public async Task CreateMotorization_AsAdmin_Returns201WithMotorizationDto()
    {
        var client = await AdminClientAsync();

        var modelResp = await client.PostAsJsonAsync("/api/catalog/models", new
        {
            Name = "Motorization Test Model",
            Brand = "Test Brand",
            Category = "Suv",
            BasePrice = 50000m,
            Description = "For motorization test",
            ImageColor = "#0000FF"
        });
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
}
