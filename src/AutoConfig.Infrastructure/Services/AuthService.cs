using AutoConfig.Core.Entities;
using AutoConfig.Core.Exceptions;
using AutoConfig.Core.Interfaces;

namespace AutoConfig.Infrastructure.Services;

public class AuthService(IUserRepository users, ITokenService tokenService) : IAuthService
{
    public async Task<(User User, string Token)> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await users.FindByEmailAsync(email, ct)
            ?? throw new UnauthorizedException("Credenziali non valide.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedException("Credenziali non valide.");

        return (user, tokenService.GenerateToken(user));
    }

    public async Task<(User User, string Token)> RegisterAsync(string name, string email, string password, CancellationToken ct = default)
    {
        if (await users.EmailExistsAsync(email, ct))
            throw new ConflictException("Email già registrata.");

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        await users.AddAsync(user, ct);
        return (user, tokenService.GenerateToken(user));
    }
}
