using System.ComponentModel.DataAnnotations;

namespace AutoConfig.Api.DTOs.Auth;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password);

public record RegisterRequest(
    [Required, MinLength(2)] string Name,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password);

public record AuthResponse(string Token, UserPayload User);

public record UserPayload(Guid Id, string Email, string Name, string Role, DateTime CreatedAt);
