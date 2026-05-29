using AutoConfig.Core.Enums;

namespace AutoConfig.Core.Entities;

public class CarOption
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Description { get; set; }
    public OptionCategory Category { get; set; }
    public decimal Price { get; set; }
    public string? Color { get; set; }

    public ICollection<CarOption> IncompatibleWith { get; init; } = [];
    public ICollection<CarOption> IncompatibleWithMe { get; init; } = [];
    public ICollection<Motorization> RequiredMotorizations { get; init; } = [];
    public ICollection<VehicleConfiguration> Configurations { get; init; } = [];
}
