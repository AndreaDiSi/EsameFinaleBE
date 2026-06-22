using System.Net;
using System.Net.Http.Json;
using AutoConfig.Api.DTOs.Auth;
using AutoConfig.Api.DTOs.Configurations;
using AutoConfig.Api.DTOs.Quotes;
using FluentAssertions;

namespace AutoConfig.Tests.Integration;

public class QuoteIntegrationTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;

    public QuoteIntegrationTests(WebAppFactory factory) => _factory = factory;

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

    private async Task<(HttpClient client, Guid configId)> SetupUserWithConfigAsync()
    {
        var client = await AuthenticatedClientAsync("giulia@example.com", "giulia123");
        var config = await (await client.PostAsJsonAsync("/api/configurations", new
        {
            Name = "Quote Test Config",
            ModelId = Model1Id,
            MotorizationId = Mot1Id,
            OptionIds = Array.Empty<Guid>()
        })).Content.ReadFromJsonAsync<ConfigurationDto>();
        return (client, config!.Id);
    }

    [Fact]
    public async Task Create_ValidConfig_Returns201WithQuoteDto()
    {
        var (client, configId) = await SetupUserWithConfigAsync();

        var response = await client.PostAsJsonAsync("/api/quotes", new
        {
            ConfigurationId = configId,
            Notes = "Test quote"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var quote = await response.Content.ReadFromJsonAsync<QuoteDto>();
        quote.Should().NotBeNull();
        quote!.ConfigurationId.Should().Be(configId);
        quote.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task GetAll_AsUser_Returns200WithList()
    {
        var (client, _) = await SetupUserWithConfigAsync();

        var response = await client.GetAsync("/api/quotes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quotes = await response.Content.ReadFromJsonAsync<List<QuoteDto>>();
        quotes.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_ExistingQuote_Returns200()
    {
        var (client, configId) = await SetupUserWithConfigAsync();
        var created = await (await client.PostAsJsonAsync("/api/quotes", new
        {
            ConfigurationId = configId,
            Notes = ""
        })).Content.ReadFromJsonAsync<QuoteDto>();

        var response = await client.GetAsync($"/api/quotes/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quote = await response.Content.ReadFromJsonAsync<QuoteDto>();
        quote!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task UpdateAdmin_ValidRequest_Returns200WithApprovedStatus()
    {
        var (userClient, configId) = await SetupUserWithConfigAsync();
        var created = await (await userClient.PostAsJsonAsync("/api/quotes", new
        {
            ConfigurationId = configId,
            Notes = ""
        })).Content.ReadFromJsonAsync<QuoteDto>();

        var adminClient = await AuthenticatedClientAsync("admin@autoconfig.it", "admin123");
        var response = await adminClient.PutAsJsonAsync($"/api/quotes/{created!.Id}", new
        {
            Status = "Approved",
            Discount = 10m,
            AdminNotes = "Approved with 10% discount"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<QuoteDto>();
        updated!.Status.Should().Be("Approved");
        updated.Discount.Should().Be(10m);
    }

    [Fact]
    public async Task Delete_ValidId_Returns204()
    {
        var (client, configId) = await SetupUserWithConfigAsync();
        var created = await (await client.PostAsJsonAsync("/api/quotes", new
        {
            ConfigurationId = configId,
            Notes = ""
        })).Content.ReadFromJsonAsync<QuoteDto>();

        var response = await client.DeleteAsync($"/api/quotes/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
