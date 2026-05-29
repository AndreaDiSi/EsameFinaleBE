using AutoConfig.Core.Enums;

namespace AutoConfig.Core.Entities;

public class CarModel
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Brand { get; set; }
    public CarCategory Category { get; set; }
    public decimal BasePrice { get; set; }
    public required string Description { get; set; }
    public required string ImageColor { get; set; }

    public ICollection<Motorization> Motorizations { get; init; } = [];
    public ICollection<VehicleConfiguration> Configurations { get; init; } = [];
}
