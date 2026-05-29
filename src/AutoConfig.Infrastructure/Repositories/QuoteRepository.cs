using AutoConfig.Core.Entities;
using AutoConfig.Core.Interfaces;
using AutoConfig.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoConfig.Infrastructure.Repositories;

public class QuoteRepository(AppDbContext db) : Repository<Quote>(db), IQuoteRepository
{
    public Task<Quote?> GetWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        Db.Quotes
            .Include(q => q.Configuration).ThenInclude(c => c.Model)
            .Include(q => q.Configuration).ThenInclude(c => c.Motorization)
            .Include(q => q.Configuration).ThenInclude(c => c.Options)
            .Include(q => q.User)
            .FirstOrDefaultAsync(q => q.Id == id, ct);

    public async Task<IReadOnlyList<Quote>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        await Db.Quotes
            .Include(q => q.Configuration).ThenInclude(c => c.Model)
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Quote>> GetAllWithDetailsAsync(CancellationToken ct = default) =>
        await Db.Quotes
            .Include(q => q.Configuration).ThenInclude(c => c.Model)
            .Include(q => q.User)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(ct);
}
