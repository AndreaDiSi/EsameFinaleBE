using AutoConfig.Api.DTOs.Configurations;
using AutoConfig.Api.Extensions;
using AutoConfig.Api.Mapping;
using AutoConfig.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoConfig.Api.Controllers;

[ApiController]
[Route("api/configurations")]
[Authorize]
public class ConfigurationsController(IConfigurationService service) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<ConfigurationDto>> GetAll(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsAdmin();

        var items = isAdmin
            ? await service.GetAllConfigurationsAsync(ct)
            : await service.GetUserConfigurationsAsync(userId, ct);

        return items.Select(c => c.ToDto()).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ConfigurationDto> Get(Guid id, CancellationToken ct) =>
        (await service.GetAsync(id, User.GetUserId(), User.IsAdmin(), ct)).ToDto();

    [HttpPost]
    public async Task<ActionResult<ConfigurationDto>> Create(CreateConfigurationRequest req, CancellationToken ct)
    {
        var config = await service.CreateAsync(
            User.GetUserId(), req.Name, req.ModelId, req.MotorizationId, req.OptionIds, ct);

        return CreatedAtAction(nameof(Get), new { id = config.Id }, config.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ConfigurationDto> Update(Guid id, UpdateConfigurationRequest req, CancellationToken ct) =>
        (await service.UpdateAsync(
            id, User.GetUserId(), User.IsAdmin(),
            new UpdateConfigurationCommand(req.Name, req.ModelId, req.MotorizationId, req.OptionIds), ct)).ToDto();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, User.GetUserId(), User.IsAdmin(), ct);
        return NoContent();
    }
}
