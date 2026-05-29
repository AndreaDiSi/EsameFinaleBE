using AutoConfig.Core.Entities;

namespace AutoConfig.Core.Interfaces;

public interface IQuoteRepository : IRepository<Quote>
{
    Task<Quote?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Quote>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Quote>> GetAllWithDetailsAsync(CancellationToken ct = default);
}
