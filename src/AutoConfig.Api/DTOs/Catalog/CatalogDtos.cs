using System.ComponentModel.DataAnnotations;

namespace AutoConfig.Api.DTOs.Catalog;

public record CarModelDto(
    Guid Id, string Name, string Brand, string Category,
    decimal BasePrice, string Description, string ImageColor,
    IReadOnlyList<MotorizationDto>? Motorizations = null);

public record MotorizationDto(
    Guid Id, Guid ModelId, string Name, string FuelType,
    int Power, int Torque, decimal Acceleration, string Consumption, decimal Price);

public record CarOptionDto(
    Guid Id, string Name, string Description, string Category,
    decimal Price, string? Color,
    IReadOnlyList<Guid> IncompatibleWith,
    IReadOnlyList<Guid> RequiredMotorizations);

public record CreateCarModelRequest(
    [Required, MinLength(2)] string Name,
    [Required, MinLength(2)] string Brand,
    [Required] string Category,
    [Required, Range(0, double.MaxValue)] decimal BasePrice,
    [Required] string Description,
    [Required] string ImageColor);

public record CreateMotorizationRequest(
    [Required] string Name,
    [Required] string FuelType,
    [Required, Range(1, 2000)] int Power,
    [Required, Range(1, 2000)] int Torque,
    [Required, Range(0, 30)] decimal Acceleration,
    [Required] string Consumption,
    [Required, Range(0, double.MaxValue)] decimal Price);
