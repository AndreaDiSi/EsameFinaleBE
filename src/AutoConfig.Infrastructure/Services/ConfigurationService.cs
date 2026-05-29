using AutoConfig.Core.Entities;
using AutoConfig.Core.Exceptions;
using AutoConfig.Core.Interfaces;

namespace AutoConfig.Infrastructure.Services;

public class ConfigurationService(
    IConfigurationRepository configurations,
    ICarModelRepository models,
    ICarOptionRepository options) : IConfigurationService
{
    public Task<IReadOnlyList<VehicleConfiguration>> GetUserConfigurationsAsync(Guid userId, CancellationToken ct = default) =>
        configurations.GetByUserAsync(userId, ct);

    public Task<IReadOnlyList<VehicleConfiguration>> GetAllConfigurationsAsync(CancellationToken ct = default) =>
        configurations.GetAllWithDetailsAsync(ct);

    public async Task<VehicleConfiguration> GetAsync(Guid id, Guid requestingUserId, bool isAdmin, CancellationToken ct = default)
    {
        var config = await configurations.GetWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("Configuration");

        if (!isAdmin && config.UserId != requestingUserId)
            throw new ForbiddenException();

        return config;
    }

    public async Task<VehicleConfiguration> CreateAsync(
        Guid userId, string name, Guid modelId, Guid motorizationId,
        IEnumerable<Guid> optionIds, CancellationToken ct = default)
    {
        var (model, motorization, selectedOptions) = await ValidateAndLoad(modelId, motorizationId, optionIds, ct);
        ValidateOptionCompatibility(selectedOptions, motorization.Id);

        var config = new VehicleConfiguration
        {
            UserId = userId,
            Name = name,
            ModelId = modelId,
            MotorizationId = motorizationId,
            TotalPrice = CalculatePrice(model, motorization, selectedOptions)
        };

        foreach (var opt in selectedOptions)
            config.Options.Add(opt);

        return await configurations.AddAsync(config, ct);
    }

    public async Task<VehicleConfiguration> UpdateAsync(
        Guid id, Guid requestingUserId, bool isAdmin,
        string name, Guid modelId, Guid motorizationId,
        IEnumerable<Guid> optionIds, CancellationToken ct = default)
    {
        var config = await configurations.GetWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("Configuration");

        if (!isAdmin && config.UserId != requestingUserId)
            throw new ForbiddenException();

        var (model, motorization, selectedOptions) = await ValidateAndLoad(modelId, motorizationId, optionIds, ct);
        ValidateOptionCompatibility(selectedOptions, motorization.Id);

        config.Name = name;
        config.ModelId = modelId;
        config.MotorizationId = motorizationId;
        config.TotalPrice = CalculatePrice(model, motorization, selectedOptions);
        config.UpdatedAt = DateTime.UtcNow;

        config.Options.Clear();
        foreach (var opt in selectedOptions)
            config.Options.Add(opt);

        await configurations.UpdateAsync(config, ct);
        return config;
    }

    public async Task DeleteAsync(Guid id, Guid requestingUserId, bool isAdmin, CancellationToken ct = default)
    {
        var config = await configurations.GetWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("Configuration");

        if (!isAdmin && config.UserId != requestingUserId)
            throw new ForbiddenException();

        await configurations.DeleteAsync(config, ct);
    }

    private async Task<(CarModel, Motorization, List<CarOption>)> ValidateAndLoad(
        Guid modelId, Guid motorizationId, IEnumerable<Guid> optionIds, CancellationToken ct)
    {
        var model = await models.GetWithMotorizationsAsync(modelId, ct)
            ?? throw new NotFoundException("CarModel");

        var motorization = model.Motorizations.FirstOrDefault(m => m.Id == motorizationId)
            ?? throw new ValidationException("La motorizzazione non appartiene al modello selezionato.");

        var idList = optionIds.ToList();
        var selectedOptions = (await options.GetByIdsWithIncompatibilitiesAsync(idList, ct)).ToList();

        if (selectedOptions.Count != idList.Count)
            throw new ValidationException("Uno o più optional non esistono.");

        return (model, motorization, selectedOptions);
    }

    private static void ValidateOptionCompatibility(List<CarOption> selectedOptions, Guid motorizationId)
    {
        var selectedIds = selectedOptions.Select(o => o.Id).ToHashSet();

        foreach (var opt in selectedOptions)
        {
            var conflict = opt.IncompatibleWith.FirstOrDefault(i => selectedIds.Contains(i.Id));
            if (conflict is not null)
                throw new ValidationException($"'{opt.Name}' è incompatibile con '{conflict.Name}'.");

            if (opt.RequiredMotorizations.Count > 0 && opt.RequiredMotorizations.All(m => m.Id != motorizationId))
                throw new ValidationException($"'{opt.Name}' non è disponibile per la motorizzazione selezionata.");
        }
    }

    private static decimal CalculatePrice(CarModel model, Motorization motorization, IEnumerable<CarOption> opts) =>
        model.BasePrice + motorization.Price + opts.Sum(o => o.Price);
}
