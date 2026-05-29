using System.Net;
using System.Net.Http.Json;
using AutoConfig.Api.DTOs.Auth;
using AutoConfig.Api.DTOs.Catalog;
using FluentAssertions;

namespace AutoConfig.Tests.Integration;

public class CatalogIntegrationTests : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client;

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
}
