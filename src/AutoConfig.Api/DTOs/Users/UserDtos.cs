using System.ComponentModel.DataAnnotations;

namespace AutoConfig.Api.DTOs.Users;

public record UserDto(Guid Id, string Email, string Name, string Role, DateTime CreatedAt);

public record UpdateUserRequest(
    [Required, MinLength(2)] string Name,
    [Required, EmailAddress] string Email,
    [Required] string Role);
