using AutoConfig.Core.Entities;
using AutoConfig.Core.Enums;

namespace AutoConfig.Core.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);
    Task<User> GetAsync(Guid id, CancellationToken ct = default);
    Task<User> UpdateAsync(Guid id, string name, string email, Role role, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
