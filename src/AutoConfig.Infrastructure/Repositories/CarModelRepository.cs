using AutoConfig.Core.Entities;
using AutoConfig.Core.Interfaces;
using AutoConfig.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoConfig.Infrastructure.Repositories;

public class CarModelRepository(AppDbContext db) : Repository<CarModel>(db), ICarModelRepository
{
    public Task<CarModel?> GetWithMotorizationsAsync(Guid id, CancellationToken ct = default) =>
        Db.CarModels.Include(m => m.Motorizations).FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<CarModel>> GetAllWithMotorizationsAsync(CancellationToken ct = default) =>
        await Db.CarModels.Include(m => m.Motorizations).ToListAsync(ct);

    public async Task<IReadOnlyList<Motorization>> GetMotorizationsByModelAsync(Guid modelId, CancellationToken ct = default) =>
        await Db.Motorizations.Where(m => m.ModelId == modelId).ToListAsync(ct);

    public Task<Motorization?> GetMotorizationByIdAsync(Guid id, CancellationToken ct = default) =>
        Db.Motorizations.FindAsync([id], ct).AsTask();

    public async Task<Motorization> AddMotorizationAsync(Motorization motorization, CancellationToken ct = default)
    {
        await Db.Motorizations.AddAsync(motorization, ct);
        await Db.SaveChangesAsync(ct);
        return motorization;
    }
}
