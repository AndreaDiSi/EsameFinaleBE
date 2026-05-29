using System.ComponentModel.DataAnnotations;

namespace AutoConfig.Api.DTOs.Configurations;

public record ConfigurationDto(
    Guid Id, Guid UserId, string Name,
    Guid ModelId, string ModelName, string ModelBrand,
    Guid MotorizationId, string MotorizationName,
    IReadOnlyList<Guid> OptionIds,
    decimal TotalPrice, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateConfigurationRequest(
    [Required, MinLength(2), MaxLength(60)] string Name,
    Guid ModelId,
    Guid MotorizationId,
    IReadOnlyList<Guid> OptionIds);

public record UpdateConfigurationRequest(
    [Required, MinLength(2), MaxLength(60)] string Name,
    Guid ModelId,
    Guid MotorizationId,
    IReadOnlyList<Guid> OptionIds);
