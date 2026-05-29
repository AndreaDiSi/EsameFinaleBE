using AutoConfig.Core.Entities;
using AutoConfig.Core.Interfaces;
using AutoConfig.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoConfig.Infrastructure.Repositories;

public class ConfigurationRepository(AppDbContext db) : Repository<VehicleConfiguration>(db), IConfigurationRepository
{
    public Task<VehicleConfiguration?> GetWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        Db.Configurations
            .Include(c => c.Model)
            .Include(c => c.Motorization)
            .Include(c => c.Options).ThenInclude(o => o.IncompatibleWith)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<VehicleConfiguration>> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        await Db.Configurations
            .Include(c => c.Model)
            .Include(c => c.Motorization)
            .Include(c => c.Options)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<VehicleConfiguration>> GetAllWithDetailsAsync(CancellationToken ct = default) =>
        await Db.Configurations
            .Include(c => c.Model)
            .Include(c => c.Motorization)
            .Include(c => c.Options)
            .Include(c => c.User)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
}
