using AutoConfig.Core.Entities;
using AutoConfig.Core.Enums;
using AutoConfig.Core.Exceptions;
using AutoConfig.Core.Interfaces;
using AutoConfig.Infrastructure.Repositories;
using AutoConfig.Infrastructure.Services;
using AutoConfig.Tests.Helpers;
using FluentAssertions;

namespace AutoConfig.Tests.Services;

public class AuthServiceTests
{
    private class StubTokenService : ITokenService
    {
        public string GenerateToken(User user) => $"token-{user.Id}";
    }

    private static AuthService CreateSut(out AutoConfig.Infrastructure.Data.AppDbContext db)
    {
        db = TestDbContextFactory.Create();
        return new AuthService(new UserRepository(db), new StubTokenService());
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsUserAndToken()
    {
        var sut = CreateSut(out var db);
        var user = TestDataBuilder.RegularUser("login@test.it");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var (resultUser, token) = await sut.LoginAsync("login@test.it", "user123");

        resultUser.Id.Should().Be(user.Id);
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsUnauthorizedException()
    {
        var sut = CreateSut(out _);

        await sut.Invoking(s => s.LoginAsync("ghost@test.it", "anypass"))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorizedException()
    {
        var sut = CreateSut(out var db);
        db.Users.Add(TestDataBuilder.RegularUser("pwd@test.it"));
        await db.SaveChangesAsync();

        await sut.Invoking(s => s.LoginAsync("pwd@test.it", "wrongpassword"))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Register_ValidData_ReturnsUserAndToken()
    {
        var sut = CreateSut(out _);

        var (user, token) = await sut.RegisterAsync("Mario Rossi", "mario@test.it", "pass123");

        user.Email.Should().Be("mario@test.it");
        user.Name.Should().Be("Mario Rossi");
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_PasswordIsStoredAsHash()
    {
        var sut = CreateSut(out var db);

        await sut.RegisterAsync("Utente", "hash@test.it", "mypassword");

        var saved = db.Users.Single(u => u.Email == "hash@test.it");
        saved.PasswordHash.Should().NotBe("mypassword");
        BCrypt.Net.BCrypt.Verify("mypassword", saved.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Register_NewUser_HasRoleUser()
    {
        var sut = CreateSut(out var db);

        await sut.RegisterAsync("Utente", "role@test.it", "pass123");

        var saved = db.Users.Single(u => u.Email == "role@test.it");
        saved.Role.Should().Be(Role.User);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsConflictException()
    {
        var sut = CreateSut(out var db);
        db.Users.Add(TestDataBuilder.RegularUser("dup@test.it"));
        await db.SaveChangesAsync();

        await sut.Invoking(s => s.RegisterAsync("Altro", "dup@test.it", "pass123"))
            .Should().ThrowAsync<ConflictException>();
    }
}
