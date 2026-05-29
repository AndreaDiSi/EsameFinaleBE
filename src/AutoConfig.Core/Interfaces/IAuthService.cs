using AutoConfig.Core.Entities;

namespace AutoConfig.Core.Interfaces;

public interface IAuthService
{
    Task<(User User, string Token)> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<(User User, string Token)> RegisterAsync(string name, string email, string password, CancellationToken ct = default);
}
