using AutoConfig.Core.Entities;

namespace AutoConfig.Core.Interfaces;

public interface ICarModelRepository : IRepository<CarModel>
{
    Task<CarModel?> GetWithMotorizationsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Motorization>> GetMotorizationsByModelAsync(Guid modelId, CancellationToken ct = default);
    Task<Motorization?> GetMotorizationByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CarModel>> GetAllWithMotorizationsAsync(CancellationToken ct = default);
    Task<Motorization> AddMotorizationAsync(Motorization motorization, CancellationToken ct = default);
}
