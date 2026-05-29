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
public class CatalogController(ICarModelRepository models, ICarOptionRepository options) : ControllerBase
{
    [HttpGet("models")]
    public async Task<IReadOnlyList<CarModelDto>> GetModels(CancellationToken ct) =>
        (await models.GetAllWithMotorizationsAsync(ct)).Select(m => m.ToDto(includeMots: true)).ToList();

    [HttpGet("models/{id:guid}")]
    public async Task<CarModelDto> GetModel(Guid id, CancellationToken ct)
    {
        var model = await models.GetWithMotorizationsAsync(id, ct)
            ?? throw new NotFoundException("CarModel");
        return model.ToDto(includeMots: true);
    }

    [HttpGet("models/{modelId:guid}/motorizations")]
    public async Task<IReadOnlyList<MotorizationDto>> GetMotorizations(Guid modelId, CancellationToken ct) =>
        (await models.GetMotorizationsByModelAsync(modelId, ct)).Select(m => m.ToDto()).ToList();

    [HttpGet("options")]
    public async Task<IReadOnlyList<CarOptionDto>> GetOptions(CancellationToken ct) =>
        (await options.GetAllWithIncompatibilitiesAsync(ct)).Select(o => o.ToDto()).ToList();

    [Authorize(Roles = "Admin")]
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
        return CreatedAtAction(nameof(GetModel), new { id = model.Id }, model.ToDto());
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("models/{id:guid}")]
    public async Task<CarModelDto> UpdateModel(Guid id, CreateCarModelRequest req, CancellationToken ct)
    {
        var model = await models.GetByIdAsync(id, ct) ?? throw new NotFoundException("CarModel");

        if (!Enum.TryParse<CarCategory>(req.Category, ignoreCase: true, out var category))
            throw new ValidationException("Categoria non valida.");

        model.Name = req.Name; model.Brand = req.Brand; model.Category = category;
        model.BasePrice = req.BasePrice; model.Description = req.Description; model.ImageColor = req.ImageColor;

        await models.UpdateAsync(model, ct);
        return model.ToDto();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("models/{id:guid}")]
    public async Task<IActionResult> DeleteModel(Guid id, CancellationToken ct)
    {
        var model = await models.GetByIdAsync(id, ct) ?? throw new NotFoundException("CarModel");
        await models.DeleteAsync(model, ct);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("models/{modelId:guid}/motorizations")]
    public async Task<ActionResult<MotorizationDto>> CreateMotorization(Guid modelId, CreateMotorizationRequest req, CancellationToken ct)
    {
        _ = await models.GetByIdAsync(modelId, ct) ?? throw new NotFoundException("CarModel");

        if (!Enum.TryParse<FuelType>(req.FuelType, ignoreCase: true, out var fuelType))
            throw new ValidationException("Tipo carburante non valido.");

        var mot = new Motorization
        {
            ModelId = modelId, Name = req.Name, FuelType = fuelType,
            Power = req.Power, Torque = req.Torque, Acceleration = req.Acceleration,
            Consumption = req.Consumption, Price = req.Price
        };

        await models.AddMotorizationAsync(mot, ct);
        return CreatedAtAction(nameof(GetMotorizations), new { modelId }, mot.ToDto());
    }
}
