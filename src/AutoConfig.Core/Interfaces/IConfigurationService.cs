using AutoConfig.Core.Entities;

namespace AutoConfig.Core.Interfaces;

public record UpdateConfigurationCommand(
    string Name, Guid ModelId, Guid MotorizationId, IEnumerable<Guid> OptionIds);

public interface IConfigurationService
{
    Task<IReadOnlyList<VehicleConfiguration>> GetUserConfigurationsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<VehicleConfiguration>> GetAllConfigurationsAsync(CancellationToken ct = default);
    Task<VehicleConfiguration> GetAsync(Guid id, Guid requestingUserId, bool isAdmin, CancellationToken ct = default);
    Task<VehicleConfiguration> CreateAsync(Guid userId, string name, Guid modelId, Guid motorizationId, IEnumerable<Guid> optionIds, CancellationToken ct = default);
    Task<VehicleConfiguration> UpdateAsync(Guid id, Guid requestingUserId, bool isAdmin, UpdateConfigurationCommand command, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid requestingUserId, bool isAdmin, CancellationToken ct = default);
}
