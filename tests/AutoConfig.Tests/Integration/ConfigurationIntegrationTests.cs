using System.Net;
using System.Net.Http.Json;
using AutoConfig.Api.DTOs.Auth;
using AutoConfig.Api.DTOs.Configurations;
using FluentAssertions;

namespace AutoConfig.Tests.Integration;

public class ConfigurationIntegrationTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;

    public ConfigurationIntegrationTests(WebAppFactory factory) => _factory = factory;

    // BMW Serie 3 (m1) + motorization 318i (mo1) — seeded by DbSeeder
    private static readonly Guid Model1Id = Guid.Parse("11111111-0000-0000-0000-000000000001");
    private static readonly Guid Mot1Id   = Guid.Parse("22222222-0000-0000-0000-000000000001");

    private async Task<HttpClient> AuthenticatedClientAsync(string email, string password)
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new("Bearer", auth!.Token);
        return client;
    }

    [Fact]
    public async Task Create_ValidRequest_Returns201WithConfigurationDto()
    {
        var client = await AuthenticatedClientAsync("mario@example.com", "mario123");

        var response = await client.PostAsJsonAsync("/api/configurations", new
        {
            Name = "My BMW Config",
            ModelId = Model1Id,
            MotorizationId = Mot1Id,
            OptionIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var config = await response.Content.ReadFromJsonAsync<ConfigurationDto>();
        config.Should().NotBeNull();
        config!.Name.Should().Be("My BMW Config");
        config.ModelId.Should().Be(Model1Id);
    }

    [Fact]
    public async Task GetAll_AsUser_Returns200WithList()
    {
        var client = await AuthenticatedClientAsync("mario@example.com", "mario123");

        var response = await client.GetAsync("/api/configurations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var configs = await response.Content.ReadFromJsonAsync<List<ConfigurationDto>>();
        configs.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_AsAdmin_Returns200WithAllConfigs()
    {
        var client = await AuthenticatedClientAsync("admin@autoconfig.it", "admin123");

        var response = await client.GetAsync("/api/configurations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var configs = await response.Content.ReadFromJsonAsync<List<ConfigurationDto>>();
        configs.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_ExistingConfig_Returns200()
    {
        var client = await AuthenticatedClientAsync("mario@example.com", "mario123");
        var created = await (await client.PostAsJsonAsync("/api/configurations", new
        {
            Name = "Get Test Config",
            ModelId = Model1Id,
            MotorizationId = Mot1Id,
            OptionIds = Array.Empty<Guid>()
        })).Content.ReadFromJsonAsync<ConfigurationDto>();

        var response = await client.GetAsync($"/api/configurations/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var config = await response.Content.ReadFromJsonAsync<ConfigurationDto>();
        config!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Update_ValidRequest_Returns200WithUpdatedName()
    {
        var client = await AuthenticatedClientAsync("mario@example.com", "mario123");
        var created = await (await client.PostAsJsonAsync("/api/configurations", new
        {
            Name = "Before Update",
            ModelId = Model1Id,
            MotorizationId = Mot1Id,
            OptionIds = Array.Empty<Guid>()
        })).Content.ReadFromJsonAsync<ConfigurationDto>();

        var response = await client.PutAsJsonAsync($"/api/configurations/{created!.Id}", new
        {
            Name = "After Update",
            ModelId = Model1Id,
            MotorizationId = Mot1Id,
            OptionIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ConfigurationDto>();
        updated!.Name.Should().Be("After Update");
    }

    [Fact]
    public async Task Delete_ValidId_Returns204()
    {
        var client = await AuthenticatedClientAsync("mario@example.com", "mario123");
        var created = await (await client.PostAsJsonAsync("/api/configurations", new
        {
            Name = "Delete Me",
            ModelId = Model1Id,
            MotorizationId = Mot1Id,
            OptionIds = Array.Empty<Guid>()
        })).Content.ReadFromJsonAsync<ConfigurationDto>();

        var response = await client.DeleteAsync($"/api/configurations/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
