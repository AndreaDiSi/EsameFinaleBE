namespace AutoConfig.Core.Entities;

public class VehicleConfiguration
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public required string Name { get; set; }
    public Guid ModelId { get; set; }
    public Guid MotorizationId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public CarModel Model { get; set; } = null!;
    public Motorization Motorization { get; set; } = null!;
    public ICollection<CarOption> Options { get; init; } = [];
    public ICollection<Quote> Quotes { get; init; } = [];
}
