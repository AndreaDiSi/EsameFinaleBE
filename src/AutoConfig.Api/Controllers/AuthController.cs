using AutoConfig.Api.DTOs.Auth;
using AutoConfig.Api.DTOs.Users;
using AutoConfig.Api.Extensions;
using AutoConfig.Api.Mapping;
using AutoConfig.Core.Enums;
using AutoConfig.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoConfig.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth, IUserService users) : ControllerBase
{
    [HttpPost("login")]
    public async Task<AuthResponse> Login(LoginRequest req, CancellationToken ct)
    {
        var (user, token) = await auth.LoginAsync(req.Email, req.Password, ct);
        return new AuthResponse(token, user.ToPayload());
    }

    [HttpPost("register")]
    public async Task<AuthResponse> Register(RegisterRequest req, CancellationToken ct)
    {
        var (user, token) = await auth.RegisterAsync(req.Name, req.Email, req.Password, ct);
        return new AuthResponse(token, user.ToPayload());
    }

    [Authorize]
    [HttpGet("me")]
    public UserPayload Me()
    {
        var userId = User.GetUserId();
        var name = User.Identity!.Name!;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)!.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value;
        return new UserPayload(userId, email, name, role, DateTime.UtcNow);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<UserPayload> UpdateProfile(UpdateProfileRequest req, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var roleStr = User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value;
        Enum.TryParse<Role>(roleStr, ignoreCase: true, out var role);
        var updated = await users.UpdateAsync(userId, req.Name, req.Email, role, ct);
        return updated.ToPayload();
    }
}
