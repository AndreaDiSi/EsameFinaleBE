using AutoConfig.Api.DTOs.Catalog;
using AutoConfig.Api.Mapping;
using AutoConfig.Core.Entities;
using AutoConfig.Core.Enums;
using AutoConfig.Core.Exceptions;
using AutoConfig.Core.Interfaces;
using AutoConfig.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoConfig.Api.Controllers;

[ApiController]
[Route("api/catalog")]
[Authorize(Roles = "Admin")]
<<<<<<< HEAD
public class AdminCatalogController(ICarModelRepository models, ICarOptionRepository options, AppDbContext db) : ControllerBase
=======
public class AdminCatalogController(ICarModelRepository models, ICarOptionRepository options) : ControllerBase
>>>>>>> 83139a211c7e1e4b4cf7931d99d8ba26ade67cb0
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

<<<<<<< HEAD
=======
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

>>>>>>> 83139a211c7e1e4b4cf7931d99d8ba26ade67cb0
    [HttpPost("options")]
    public async Task<ActionResult<CarOptionDto>> CreateOption(CreateCarOptionRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<OptionCategory>(req.Category, ignoreCase: true, out var category))
<<<<<<< HEAD
            throw new ValidationException("Categoria opzione non valida.");

        var incompatibles = req.IncompatibleWith.Count > 0
            ? await db.CarOptions.Where(o => req.IncompatibleWith.Contains(o.Id)).ToListAsync(ct)
            : [];

        var motorizations = req.RequiredMotorizationIds.Count > 0
            ? await db.Motorizations.Where(m => req.RequiredMotorizationIds.Contains(m.Id)).ToListAsync(ct)
            : [];
=======
            throw new ValidationException("Categoria optional non valida.");
>>>>>>> 83139a211c7e1e4b4cf7931d99d8ba26ade67cb0

        var option = new CarOption
        {
            Name = req.Name, Description = req.Description,
            Category = category, Price = req.Price, Color = req.Color
        };

<<<<<<< HEAD
        foreach (var inc in incompatibles) option.IncompatibleWith.Add(inc);
        foreach (var mot in motorizations) option.RequiredMotorizations.Add(mot);

        await options.AddAsync(option, ct);
        return CreatedAtAction("GetOptions", "CarOptions", null, option.ToDto());
=======
        if (req.IncompatibleWith?.Count > 0)
        {
            var incompatibles = await options.GetByIdsWithIncompatibilitiesAsync(req.IncompatibleWith, ct);
            foreach (var inc in incompatibles)
                option.IncompatibleWith.Add(inc);
        }

        if (req.RequiredMotorizationIds?.Count > 0)
        {
            var motorizations = await models.GetMotorizationsByIdsAsync(req.RequiredMotorizationIds, ct);
            foreach (var m in motorizations)
                option.RequiredMotorizations.Add(m);
        }

        await options.AddAsync(option, ct);
        return StatusCode(201, option.ToDto());
>>>>>>> 83139a211c7e1e4b4cf7931d99d8ba26ade67cb0
    }

    [HttpPut("options/{id:guid}")]
    public async Task<CarOptionDto> UpdateOption(Guid id, CreateCarOptionRequest req, CancellationToken ct)
    {
<<<<<<< HEAD
        var option = await options.GetWithIncompatibilitiesAsync(id, ct) ?? throw new NotFoundException("CarOption");

        if (!Enum.TryParse<OptionCategory>(req.Category, ignoreCase: true, out var category))
            throw new ValidationException("Categoria opzione non valida.");
=======
        var option = await options.GetWithIncompatibilitiesAsync(id, ct)
            ?? throw new NotFoundException(CarOptionEntity);

        if (!Enum.TryParse<OptionCategory>(req.Category, ignoreCase: true, out var category))
            throw new ValidationException("Categoria optional non valida.");
>>>>>>> 83139a211c7e1e4b4cf7931d99d8ba26ade67cb0

        option.Name = req.Name; option.Description = req.Description;
        option.Category = category; option.Price = req.Price; option.Color = req.Color;

        option.IncompatibleWith.Clear();
<<<<<<< HEAD
        if (req.IncompatibleWith.Count > 0)
        {
            var incompatibles = await db.CarOptions.Where(o => req.IncompatibleWith.Contains(o.Id)).ToListAsync(ct);
            foreach (var inc in incompatibles) option.IncompatibleWith.Add(inc);
        }

        option.RequiredMotorizations.Clear();
        if (req.RequiredMotorizationIds.Count > 0)
        {
            var motorizations = await db.Motorizations.Where(m => req.RequiredMotorizationIds.Contains(m.Id)).ToListAsync(ct);
            foreach (var mot in motorizations) option.RequiredMotorizations.Add(mot);
        }

        await db.SaveChangesAsync(ct);
=======
        if (req.IncompatibleWith?.Count > 0)
        {
            var incompatibles = await options.GetByIdsWithIncompatibilitiesAsync(req.IncompatibleWith, ct);
            foreach (var inc in incompatibles)
                option.IncompatibleWith.Add(inc);
        }

        option.RequiredMotorizations.Clear();
        if (req.RequiredMotorizationIds?.Count > 0)
        {
            var motorizations = await models.GetMotorizationsByIdsAsync(req.RequiredMotorizationIds, ct);
            foreach (var m in motorizations)
                option.RequiredMotorizations.Add(m);
        }

        await options.UpdateAsync(option, ct);
>>>>>>> 83139a211c7e1e4b4cf7931d99d8ba26ade67cb0
        return option.ToDto();
    }

    [HttpDelete("options/{id:guid}")]
    public async Task<IActionResult> DeleteOption(Guid id, CancellationToken ct)
    {
<<<<<<< HEAD
        var option = await options.GetByIdAsync(id, ct) ?? throw new NotFoundException("CarOption");
=======
        var option = await options.GetByIdAsync(id, ct) ?? throw new NotFoundException(CarOptionEntity);
>>>>>>> 83139a211c7e1e4b4cf7931d99d8ba26ade67cb0
        await options.DeleteAsync(option, ct);
        return NoContent();
    }
}
