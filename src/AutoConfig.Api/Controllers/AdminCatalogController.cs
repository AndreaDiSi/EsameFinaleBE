using AutoConfig.Api.DTOs.Catalog;
using AutoConfig.Api.Mapping;
using AutoConfig.Core.Entities;
using AutoConfig.Core.Enums;
using AutoConfig.Core.Exceptions;
using AutoConfig.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoConfig.Api.Controllers;

[ApiController]
[Route("api/catalog")]
[Authorize(Roles = "Admin")]
public class AdminCatalogController(ICarModelRepository models, ICarOptionRepository options) : ControllerBase
{
    private const string CarModelEntity = "CarModel";
    private const string MotorizationEntity = "Motorization";
    private const string CarOptionEntity = "CarOption";

    [HttpPost("models")]
    public async Task<ActionResult<CarModelDto>> CreateModel(CreateCarModelRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<CarCategory>(req.Category, ignoreCase: true, out var category))
            throw new ValidationException("Categoria non valida.");

        var model = new CarModel
        {
            Name = req.Name, Brand = req.Brand, Category = category,
            BasePrice = req.BasePrice, Description = req.Description, ImageColor = req.ImageColor
        };

        await models.AddAsync(model, ct);
        return CreatedAtAction("GetModel", "CarModels", new { id = model.Id }, model.ToDto());
    }

    [HttpPut("models/{id:guid}")]
    public async Task<CarModelDto> UpdateModel(Guid id, CreateCarModelRequest req, CancellationToken ct)
    {
        var model = await models.GetByIdAsync(id, ct) ?? throw new NotFoundException(CarModelEntity);

        if (!Enum.TryParse<CarCategory>(req.Category, ignoreCase: true, out var category))
            throw new ValidationException("Categoria non valida.");

        model.Name = req.Name; model.Brand = req.Brand; model.Category = category;
        model.BasePrice = req.BasePrice; model.Description = req.Description; model.ImageColor = req.ImageColor;

        await models.UpdateAsync(model, ct);
        return model.ToDto();
    }

    [HttpDelete("models/{id:guid}")]
    public async Task<IActionResult> DeleteModel(Guid id, CancellationToken ct)
    {
        var model = await models.GetByIdAsync(id, ct) ?? throw new NotFoundException(CarModelEntity);
        await models.DeleteAsync(model, ct);
        return NoContent();
    }

    [HttpPost("models/{modelId:guid}/motorizations")]
    public async Task<ActionResult<MotorizationDto>> CreateMotorization(Guid modelId, CreateMotorizationRequest req, CancellationToken ct)
    {
        _ = await models.GetByIdAsync(modelId, ct) ?? throw new NotFoundException(CarModelEntity);

        if (!Enum.TryParse<FuelType>(req.FuelType, ignoreCase: true, out var fuelType))
            throw new ValidationException("Tipo carburante non valido.");

        var mot = new Motorization
        {
            ModelId = modelId, Name = req.Name, FuelType = fuelType,
            Power = req.Power, Torque = req.Torque, Acceleration = req.Acceleration,
            Consumption = req.Consumption, Price = req.Price
        };

        await models.AddMotorizationAsync(mot, ct);
        return CreatedAtAction("GetMotorizations", "CarModels", new { modelId }, mot.ToDto());
    }

    [HttpPut("models/{modelId:guid}/motorizations/{id:guid}")]
    public async Task<MotorizationDto> UpdateMotorization(Guid modelId, Guid id, CreateMotorizationRequest req, CancellationToken ct)
    {
        _ = await models.GetByIdAsync(modelId, ct) ?? throw new NotFoundException(CarModelEntity);
        var mot = await models.GetMotorizationByIdAsync(id, ct) ?? throw new NotFoundException(MotorizationEntity);
        if (mot.ModelId != modelId) throw new NotFoundException(MotorizationEntity);

        if (!Enum.TryParse<FuelType>(req.FuelType, ignoreCase: true, out var fuelType))
            throw new ValidationException("Tipo carburante non valido.");

        mot.Name = req.Name; mot.FuelType = fuelType;
        mot.Power = req.Power; mot.Torque = req.Torque;
        mot.Acceleration = req.Acceleration; mot.Consumption = req.Consumption; mot.Price = req.Price;

        await models.UpdateMotorizationAsync(mot, ct);
        return mot.ToDto();
    }

    [HttpDelete("models/{modelId:guid}/motorizations/{id:guid}")]
    public async Task<IActionResult> DeleteMotorization(Guid modelId, Guid id, CancellationToken ct)
    {
        _ = await models.GetByIdAsync(modelId, ct) ?? throw new NotFoundException(CarModelEntity);
        var mot = await models.GetMotorizationByIdAsync(id, ct) ?? throw new NotFoundException(MotorizationEntity);
        if (mot.ModelId != modelId) throw new NotFoundException(MotorizationEntity);

        await models.DeleteMotorizationAsync(mot, ct);
        return NoContent();
    }

    [HttpPost("options")]
    public async Task<ActionResult<CarOptionDto>> CreateOption(CreateCarOptionRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<OptionCategory>(req.Category, ignoreCase: true, out var category))
            throw new ValidationException("Categoria opzione non valida.");

        var option = new CarOption
        {
            Name = req.Name, Description = req.Description,
            Category = category, Price = req.Price, Color = req.Color
        };

        if (req.IncompatibleWith?.Count > 0)
        {
            var incompatibles = await options.GetByIdsWithIncompatibilitiesAsync(req.IncompatibleWith, ct);
            foreach (var inc in incompatibles) option.IncompatibleWith.Add(inc);
        }

        if (req.RequiredMotorizationIds?.Count > 0)
        {
            var motorizations = await models.GetMotorizationsByIdsAsync(req.RequiredMotorizationIds, ct);
            foreach (var mot in motorizations) option.RequiredMotorizations.Add(mot);
        }

        await options.AddAsync(option, ct);
        return CreatedAtAction("GetOptions", "CarOptions", null, option.ToDto());
    }

    [HttpPut("options/{id:guid}")]
    public async Task<CarOptionDto> UpdateOption(Guid id, CreateCarOptionRequest req, CancellationToken ct)
    {
        var option = await options.GetWithIncompatibilitiesAsync(id, ct)
            ?? throw new NotFoundException(CarOptionEntity);

        if (!Enum.TryParse<OptionCategory>(req.Category, ignoreCase: true, out var category))
            throw new ValidationException("Categoria opzione non valida.");

        option.Name = req.Name; option.Description = req.Description;
        option.Category = category; option.Price = req.Price; option.Color = req.Color;

        option.IncompatibleWith.Clear();
        if (req.IncompatibleWith?.Count > 0)
        {
            var incompatibles = await options.GetByIdsWithIncompatibilitiesAsync(req.IncompatibleWith, ct);
            foreach (var inc in incompatibles) option.IncompatibleWith.Add(inc);
        }

        option.RequiredMotorizations.Clear();
        if (req.RequiredMotorizationIds?.Count > 0)
        {
            var motorizations = await models.GetMotorizationsByIdsAsync(req.RequiredMotorizationIds, ct);
            foreach (var mot in motorizations) option.RequiredMotorizations.Add(mot);
        }

        await options.UpdateAsync(option, ct);
        return option.ToDto();
    }

    [HttpDelete("options/{id:guid}")]
    public async Task<IActionResult> DeleteOption(Guid id, CancellationToken ct)
    {
        var option = await options.GetByIdAsync(id, ct) ?? throw new NotFoundException(CarOptionEntity);
        await options.DeleteAsync(option, ct);
        return NoContent();
    }
}
