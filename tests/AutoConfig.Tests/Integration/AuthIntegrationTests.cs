using System.Net;
using System.Net.Http.Json;
using AutoConfig.Api.DTOs.Auth;
using FluentAssertions;

namespace AutoConfig.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidRequest_Returns200WithToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Name = "Test User",
            Email = "newuser@integration.it",
            Password = "password123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
        body.User.Email.Should().Be("newuser@integration.it");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var payload = new { Name = "Dup", Email = "dup-integration@test.it", Password = "pass123456" };

        var first = await _client.PostAsJsonAsync("/api/auth/register", payload);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await _client.PostAsJsonAsync("/api/auth/register", payload);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var reg = await _client.PostAsJsonAsync("/api/auth/register",
            new { Name = "Login Test", Email = "logintest-integration@test.it", Password = "pass123456" });
        reg.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { Email = "logintest-integration@test.it", Password = "pass123456" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        await _client.PostAsJsonAsync("/api/auth/register",
            new { Name = "Auth Test", Email = "wrongpwd-integration@test.it", Password = "correctpass" });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { Email = "wrongpwd-integration@test.it", Password = "wrongpass!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithToken_Returns200()
    {
        var reg = await _client.PostAsJsonAsync("/api/auth/register",
            new { Name = "Me Test", Email = "me-integration@test.it", Password = "pass123456" });
        var auth = await reg.Content.ReadFromJsonAsync<AuthResponse>();

        var authedClient = _factory.CreateClient();
        authedClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.Token);

        var response = await authedClient.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
