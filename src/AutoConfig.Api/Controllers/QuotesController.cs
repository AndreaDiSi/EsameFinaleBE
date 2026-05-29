using AutoConfig.Api.DTOs.Quotes;
using AutoConfig.Api.Extensions;
using AutoConfig.Api.Mapping;
using AutoConfig.Core.Enums;
using AutoConfig.Core.Exceptions;
using AutoConfig.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoConfig.Api.Controllers;

[ApiController]
[Route("api/quotes")]
[Authorize]
public class QuotesController(IQuoteService service) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<QuoteDto>> GetAll(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsAdmin();

        var items = isAdmin
            ? await service.GetAllQuotesAsync(ct)
            : await service.GetUserQuotesAsync(userId, ct);

        return items.Select(q => q.ToDto()).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<QuoteDto> Get(Guid id, CancellationToken ct) =>
        (await service.GetAsync(id, User.GetUserId(), User.IsAdmin(), ct)).ToDto();

    [HttpPost]
    public async Task<ActionResult<QuoteDto>> Create(CreateQuoteRequest req, CancellationToken ct)
    {
        var quote = await service.CreateAsync(User.GetUserId(), req.ConfigurationId, req.Notes, ct);
        return CreatedAtAction(nameof(Get), new { id = quote.Id }, quote.ToDto());
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<QuoteDto> UpdateAdmin(Guid id, UpdateQuoteAdminRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<QuoteStatus>(req.Status, ignoreCase: true, out var status))
            throw new ValidationException("Stato non valido.");

        return (await service.UpdateAdminAsync(id, status, req.Discount, req.AdminNotes, ct)).ToDto();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, User.GetUserId(), User.IsAdmin(), ct);
        return NoContent();
    }
}
