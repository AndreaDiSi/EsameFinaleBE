using AutoConfig.Api.DTOs.Catalog;
using AutoConfig.Api.Mapping;
using AutoConfig.Core.Exceptions;
using AutoConfig.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoConfig.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController(ICarModelRepository models, ICarOptionRepository options) : ControllerBase
{
    private const string CarModelEntity = "CarModel";

    [HttpGet("models")]
    public async Task<IReadOnlyList<CarModelDto>> GetModels(CancellationToken ct) =>
        (await models.GetAllWithMotorizationsAsync(ct)).Select(m => m.ToDto(includeMots: true)).ToList();

    [HttpGet("models/{id:guid}")]
    public async Task<CarModelDto> GetModel(Guid id, CancellationToken ct)
    {
        var model = await models.GetWithMotorizationsAsync(id, ct)
            ?? throw new NotFoundException(CarModelEntity);
        return model.ToDto(includeMots: true);
    }

    [HttpGet("models/{modelId:guid}/motorizations")]
    public async Task<IReadOnlyList<MotorizationDto>> GetMotorizations(Guid modelId, CancellationToken ct) =>
        (await models.GetMotorizationsByModelAsync(modelId, ct)).Select(m => m.ToDto()).ToList();

    [HttpGet("options")]
    public async Task<IReadOnlyList<CarOptionDto>> GetOptions(CancellationToken ct) =>
        (await options.GetAllWithIncompatibilitiesAsync(ct)).Select(o => o.ToDto()).ToList();
}
