using AutoConfig.Api.DTOs.Auth;
using AutoConfig.Api.Extensions;
using AutoConfig.Api.Mapping;
using AutoConfig.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoConfig.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
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
}
