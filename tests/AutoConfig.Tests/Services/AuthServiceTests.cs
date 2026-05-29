using AutoConfig.Core.Exceptions;
using AutoConfig.Infrastructure.Repositories;
using AutoConfig.Infrastructure.Services;
using AutoConfig.Tests.Helpers;
using FluentAssertions;
using Moq;
using AutoConfig.Core.Interfaces;

namespace AutoConfig.Tests.Services;

public class AuthServiceTests
{
    private readonly AuthService _sut;
    private readonly Mock<ITokenService> _tokenMock = new();

    public AuthServiceTests()
    {
        var db = TestDbContextFactory.Create();
        var repo = new UserRepository(db);
        _tokenMock.Setup(t => t.GenerateToken(It.IsAny<Core.Entities.User>())).Returns("test-token");
        _sut = new AuthService(repo, _tokenMock.Object);
    }

    [Fact]
    public async Task Register_ValidData_ReturnsUserAndToken()
    {
        var (user, token) = await _sut.RegisterAsync("Mario Rossi", "mario@test.it", "password123");

        user.Email.Should().Be("mario@test.it");
        user.Name.Should().Be("Mario Rossi");
        token.Should().Be("test-token");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsConflictException()
    {
        await _sut.RegisterAsync("User One", "dup@test.it", "pass123");

        await _sut.Invoking(s => s.RegisterAsync("User Two", "dup@test.it", "pass123"))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsUserAndToken()
    {
        await _sut.RegisterAsync("Test User", "login@test.it", "mypassword");

        var (user, token) = await _sut.LoginAsync("login@test.it", "mypassword");

        user.Email.Should().Be("login@test.it");
        token.Should().Be("test-token");
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorizedException()
    {
        await _sut.RegisterAsync("Test User", "auth@test.it", "correct123");

        await _sut.Invoking(s => s.LoginAsync("auth@test.it", "wrong123"))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Login_NonExistentEmail_ThrowsUnauthorizedException()
    {
        await _sut.Invoking(s => s.LoginAsync("nobody@test.it", "pass"))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Register_PasswordIsHashed()
    {
        var (user, _) = await _sut.RegisterAsync("Hash Test", "hash@test.it", "plaintext");

        user.PasswordHash.Should().NotBe("plaintext");
        BCrypt.Net.BCrypt.Verify("plaintext", user.PasswordHash).Should().BeTrue();
    }
}
