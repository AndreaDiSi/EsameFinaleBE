using AutoConfig.Core.Entities;
using AutoConfig.Core.Enums;

namespace AutoConfig.Core.Interfaces;

public interface IQuoteService
{
    Task<IReadOnlyList<Quote>> GetUserQuotesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Quote>> GetAllQuotesAsync(CancellationToken ct = default);
    Task<Quote> GetAsync(Guid id, Guid requestingUserId, bool isAdmin, CancellationToken ct = default);
    Task<Quote> CreateAsync(Guid userId, Guid configurationId, string notes, CancellationToken ct = default);
    Task<Quote> UpdateAdminAsync(Guid id, QuoteStatus status, decimal discount, string adminNotes, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid requestingUserId, bool isAdmin, CancellationToken ct = default);
}
