using AutoConfig.Core.Enums;

namespace AutoConfig.Core.Entities;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string PasswordHash { get; set; }
    public Role Role { get; set; } = Role.User;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ICollection<VehicleConfiguration> Configurations { get; init; } = [];
    public ICollection<Quote> Quotes { get; init; } = [];
}
