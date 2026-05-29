using AutoConfig.Core.Entities;
using AutoConfig.Core.Enums;
using AutoConfig.Core.Exceptions;
using AutoConfig.Core.Interfaces;

namespace AutoConfig.Infrastructure.Services;

public class UserService(IUserRepository users) : IUserService
{
    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) =>
        users.GetAllAsync(ct);

    public async Task<User> GetAsync(Guid id, CancellationToken ct = default) =>
        await users.GetByIdAsync(id, ct) ?? throw new NotFoundException("User");

    public async Task<User> UpdateAsync(Guid id, string name, string email, Role role, CancellationToken ct = default)
    {
        var user = await GetAsync(id, ct);

        var emailOwner = await users.FindByEmailAsync(email, ct);
        if (emailOwner is not null && emailOwner.Id != id)
            throw new ConflictException("Email già in uso.");

        user.Name = name;
        user.Email = email;
        user.Role = role;

        await users.UpdateAsync(user, ct);
        return user;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await GetAsync(id, ct);
        await users.DeleteAsync(user, ct);
    }
}
