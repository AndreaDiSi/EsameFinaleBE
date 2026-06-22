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
public class AdminCatalogController(ICarModelRepository models) : ControllerBase
{
    private const string CarModelEntity = "CarModel";

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
}
