using AutoConfig.Core.Entities;

namespace AutoConfig.Core.Interfaces;

public interface ICarOptionRepository : IRepository<CarOption>
{
    Task<IReadOnlyList<CarOption>> GetAllWithIncompatibilitiesAsync(CancellationToken ct = default);
    Task<CarOption?> GetWithIncompatibilitiesAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CarOption>> GetByIdsWithIncompatibilitiesAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
