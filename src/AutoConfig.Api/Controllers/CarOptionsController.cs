using AutoConfig.Api.DTOs.Catalog;
using AutoConfig.Api.Mapping;
using AutoConfig.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoConfig.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public class CarOptionsController(ICarOptionRepository options) : ControllerBase
{
    [HttpGet("options")]
    public async Task<IReadOnlyList<CarOptionDto>> GetOptions(CancellationToken ct) =>
        (await options.GetAllWithIncompatibilitiesAsync(ct)).Select(o => o.ToDto()).ToList();
}
