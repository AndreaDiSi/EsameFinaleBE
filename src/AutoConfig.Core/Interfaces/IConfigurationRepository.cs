using AutoConfig.Core.Entities;

namespace AutoConfig.Core.Interfaces;

public interface IConfigurationRepository : IRepository<VehicleConfiguration>
{
    Task<VehicleConfiguration?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<VehicleConfiguration>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<VehicleConfiguration>> GetAllWithDetailsAsync(CancellationToken ct = default);
}
