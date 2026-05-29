using AutoConfig.Core.Entities;
using AutoConfig.Core.Enums;
using AutoConfig.Core.Exceptions;
using AutoConfig.Core.Interfaces;

namespace AutoConfig.Infrastructure.Services;

public class QuoteService(IQuoteRepository quotes, IConfigurationRepository configurations) : IQuoteService
{
    public Task<IReadOnlyList<Quote>> GetUserQuotesAsync(Guid userId, CancellationToken ct = default) =>
        quotes.GetByUserAsync(userId, ct);

    public Task<IReadOnlyList<Quote>> GetAllQuotesAsync(CancellationToken ct = default) =>
        quotes.GetAllWithDetailsAsync(ct);

    public async Task<Quote> GetAsync(Guid id, Guid requestingUserId, bool isAdmin, CancellationToken ct = default)
    {
        var quote = await quotes.GetWithDetailsAsync(id, ct)
            ?? throw new NotFoundException("Quote");

        if (!isAdmin && quote.UserId != requestingUserId)
            throw new ForbiddenException();

        return quote;
    }

    public async Task<Quote> CreateAsync(Guid userId, Guid configurationId, string notes, CancellationToken ct = default)
    {
        var config = await configurations.GetWithDetailsAsync(configurationId, ct)
            ?? throw new NotFoundException("Configuration");

        if (config.UserId != userId)
            throw new ForbiddenException("Non puoi richiedere un preventivo per una configurazione altrui.");

        var quote = new Quote
        {
            ConfigurationId = configurationId,
            UserId = userId,
            TotalPrice = config.TotalPrice,
            Discount = 0,
            FinalPrice = config.TotalPrice,
            Notes = notes,
            ExpiresAt = DateTime.UtcNow.AddDays(90)
        };

        return await quotes.AddAsync(quote, ct);
    }

    public async Task<Quote> UpdateAdminAsync(Guid id, QuoteStatus status, decimal discount, string adminNotes, CancellationToken ct = default)
    {
        var quote = await quotes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Quote");

        quote.Status = status;
        quote.Discount = discount;
        quote.FinalPrice = quote.TotalPrice * (1 - discount / 100m);
        quote.AdminNotes = adminNotes;
        quote.UpdatedAt = DateTime.UtcNow;

        await quotes.UpdateAsync(quote, ct);
        return quote;
    }

    public async Task DeleteAsync(Guid id, Guid requestingUserId, bool isAdmin, CancellationToken ct = default)
    {
        var quote = await quotes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Quote");

        if (!isAdmin && quote.UserId != requestingUserId)
            throw new ForbiddenException();

        await quotes.DeleteAsync(quote, ct);
    }
}
