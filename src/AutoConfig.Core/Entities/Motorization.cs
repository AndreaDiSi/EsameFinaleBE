using AutoConfig.Core.Enums;

namespace AutoConfig.Core.Entities;

public class Motorization
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ModelId { get; set; }
    public required string Name { get; set; }
    public FuelType FuelType { get; set; }
    public int Power { get; set; }
    public int Torque { get; set; }
    public decimal Acceleration { get; set; }
    public required string Consumption { get; set; }
    public decimal Price { get; set; }

    public CarModel Model { get; set; } = null!;
    public ICollection<VehicleConfiguration> Configurations { get; init; } = [];
    public ICollection<CarOption> RequiredByOptions { get; init; } = [];
}
