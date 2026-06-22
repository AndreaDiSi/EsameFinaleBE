using System.Net;
using System.Net.Http.Json;
using AutoConfig.Api.DTOs.Auth;
using AutoConfig.Api.DTOs.Catalog;
using FluentAssertions;

namespace AutoConfig.Tests.Integration;

public class CatalogIntegrationTests : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;

    // Seeded BMW Serie 3 (DbSeeder)
    private static readonly Guid SeededModelId = Guid.Parse("11111111-0000-0000-0000-000000000001");

    public CatalogIntegrationTests(WebAppFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetModels_NoAuth_Returns200()
    {
        var response = await _client.GetAsync("/api/catalog/models");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var models = await response.Content.ReadFromJsonAsync<List<CarModelDto>>();
        models.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOptions_NoAuth_Returns200()
    {
        var response = await _client.GetAsync("/api/catalog/options");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var options = await response.Content.ReadFromJsonAsync<List<CarOptionDto>>();
        options.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMotorizations_ValidModel_ReturnsMotorizations()
    {
        var modelsResp = await _client.GetAsync("/api/catalog/models");
        var models = await modelsResp.Content.ReadFromJsonAsync<List<CarModelDto>>();
        var firstModel = models!.First();

        var response = await _client.GetAsync($"/api/catalog/models/{firstModel.Id}/motorizations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var mots = await response.Content.ReadFromJsonAsync<List<MotorizationDto>>();
        mots.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateModel_WithoutAdminRole_Returns403()
    {
        var reg = await _client.PostAsJsonAsync("/api/auth/register",
            new { Name = "User", Email = "catalog-user@test.it", Password = "pass123456" });
        var auth = await reg.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.Token);

        var response = await _client.PostAsJsonAsync("/api/catalog/models", new
        {
            Name = "Hacked Model", Brand = "Bad", Category = "sedan",
            BasePrice = 1, Description = "X", ImageColor = "#000"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetModel_ById_NoAuth_Returns200WithMotorizations()
    {
        var response = await _client.GetAsync($"/api/catalog/models/{SeededModelId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var model = await response.Content.ReadFromJsonAsync<CarModelDto>();
        model.Should().NotBeNull();
        model!.Id.Should().Be(SeededModelId);
        model.Name.Should().Be("Serie 3");
        model.Brand.Should().Be("BMW");
        model.Motorizations.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetModel_NonExistentId_Returns404()
    {
        var response = await _client.GetAsync($"/api/catalog/models/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMotorizations_NonExistentModelId_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync($"/api/catalog/models/{Guid.NewGuid()}/motorizations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var mots = await response.Content.ReadFromJsonAsync<List<MotorizationDto>>();
        mots.Should().BeEmpty();
    }
}
