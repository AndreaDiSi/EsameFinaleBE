using System.Net;
using System.Net.Http.Json;
using AutoConfig.Api.DTOs.Admin;
using AutoConfig.Api.DTOs.Auth;
using AutoConfig.Api.DTOs.Users;
using FluentAssertions;

namespace AutoConfig.Tests.Integration;

public class UsersAdminIntegrationTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;

    // Seeded IDs (DbSeeder)
    private static readonly Guid SeededMarioId = Guid.Parse("44444444-0000-0000-0000-000000000002");

    public UsersAdminIntegrationTests(WebAppFactory factory) => _factory = factory;

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

    private async Task<Guid> RegisterAndGetIdAsync(string email)
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/register",
            new { Name = "Test User", Email = email, Password = "password123" });
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.User.Id;
    }

    // ── UsersController ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_AsAdmin_Returns200WithUserList()
    {
        var client = await AdminClientAsync();

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users!.Count.Should().BeGreaterThanOrEqualTo(3); // at least the 3 seeded users
    }

    [Fact]
    public async Task Get_ExistingUser_AsAdmin_Returns200()
    {
        var client = await AdminClientAsync();

        var response = await client.GetAsync($"/api/users/{SeededMarioId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user!.Id.Should().Be(SeededMarioId);
        user.Email.Should().Be("mario@example.com");
    }

    [Fact]
    public async Task Get_NonExistentUser_AsAdmin_Returns404()
    {
        var client = await AdminClientAsync();

        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_AsAdmin_ChangesNameAndRole()
    {
        var userId = await RegisterAndGetIdAsync("update-user-admin@test.it");
        var client = await AdminClientAsync();

        var response = await client.PutAsJsonAsync($"/api/users/{userId}", new
        {
            Name = "Updated Name",
            Email = "update-user-admin@test.it",
            Role = "Admin"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<UserDto>();
        updated!.Name.Should().Be("Updated Name");
        updated.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Update_InvalidRole_Returns422()
    {
        var userId = await RegisterAndGetIdAsync("invalid-role-admin@test.it");
        var client = await AdminClientAsync();

        var response = await client.PutAsJsonAsync($"/api/users/{userId}", new
        {
            Name = "Test",
            Email = "invalid-role-admin@test.it",
            Role = "SuperUser"
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Update_NonExistentUser_Returns404()
    {
        var client = await AdminClientAsync();

        var response = await client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", new
        {
            Name = "Ghost",
            Email = "ghost@test.it",
            Role = "User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_AsAdmin_Returns204()
    {
        var userId = await RegisterAndGetIdAsync("delete-me-admin@test.it");
        var client = await AdminClientAsync();

        var response = await client.DeleteAsync($"/api/users/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_NonExistentUser_Returns404()
    {
        var client = await AdminClientAsync();

        var response = await client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_AsRegularUser_Returns403()
    {
        var client = await UserClientAsync();

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── AdminController (stats) ───────────────────────────────────────────────

    [Fact]
    public async Task GetStats_AsAdmin_Returns200WithDashboardStats()
    {
        var client = await AdminClientAsync();

        var response = await client.GetAsync("/api/admin/stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<DashboardStatsDto>();
        stats.Should().NotBeNull();
        stats!.TotalUsers.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetStats_AsRegularUser_Returns403()
    {
        var client = await UserClientAsync();

        var response = await client.GetAsync("/api/admin/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetStats_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/admin/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
