using AutoConfig.Core.Entities;
using AutoConfig.Core.Interfaces;
using AutoConfig.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoConfig.Infrastructure.Repositories;

public class CarOptionRepository(AppDbContext db) : Repository<CarOption>(db), ICarOptionRepository
{
    public async Task<IReadOnlyList<CarOption>> GetAllWithIncompatibilitiesAsync(CancellationToken ct = default) =>
        await Db.CarOptions
            .Include(o => o.IncompatibleWith)
            .Include(o => o.RequiredMotorizations)
            .ToListAsync(ct);

    public Task<CarOption?> GetWithIncompatibilitiesAsync(Guid id, CancellationToken ct = default) =>
        Db.CarOptions
            .Include(o => o.IncompatibleWith)
            .Include(o => o.RequiredMotorizations)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<CarOption>> GetByIdsWithIncompatibilitiesAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await Db.CarOptions
            .Include(o => o.IncompatibleWith)
            .Include(o => o.RequiredMotorizations)
            .Where(o => idList.Contains(o.Id))
            .ToListAsync(ct);
    }
}
