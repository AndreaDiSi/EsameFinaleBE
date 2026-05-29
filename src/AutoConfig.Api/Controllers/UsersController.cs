using AutoConfig.Api.DTOs.Users;
using AutoConfig.Api.Mapping;
using AutoConfig.Core.Enums;
using AutoConfig.Core.Exceptions;
using AutoConfig.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoConfig.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController(IUserService users) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<UserDto>> GetAll(CancellationToken ct) =>
        (await users.GetAllAsync(ct)).Select(u => u.ToDto()).ToList();

    [HttpGet("{id:guid}")]
    public async Task<UserDto> Get(Guid id, CancellationToken ct) =>
        (await users.GetAsync(id, ct)).ToDto();

    [HttpPut("{id:guid}")]
    public async Task<UserDto> Update(Guid id, UpdateUserRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<Role>(req.Role, ignoreCase: true, out var role))
            throw new ValidationException("Ruolo non valido.");

        return (await users.UpdateAsync(id, req.Name, req.Email, role, ct)).ToDto();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await users.DeleteAsync(id, ct);
        return NoContent();
    }
}
