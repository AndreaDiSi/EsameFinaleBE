using AutoConfig.Api.DTOs.Admin;
using AutoConfig.Core.Enums;
using AutoConfig.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoConfig.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext db) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<DashboardStatsDto> GetStats(CancellationToken ct)
    {
        var totalUsers = await db.Users.CountAsync(ct);
        var totalConfigs = await db.Configurations.CountAsync(ct);
        var quotes = await db.Quotes.ToListAsync(ct);

        var pendingQuotes = quotes.Count(q => q.Status == QuoteStatus.Pending);
        var approvedQuotes = quotes.Count(q => q.Status == QuoteStatus.Approved);
        var totalRevenue = quotes.Where(q => q.Status == QuoteStatus.Approved).Sum(q => q.FinalPrice);

        var avgPrice = totalConfigs > 0
            ? await db.Configurations.AverageAsync(c => (double)c.TotalPrice, ct)
            : 0;

        return new DashboardStatsDto(
            totalUsers, totalConfigs, quotes.Count,
            pendingQuotes, approvedQuotes,
            totalRevenue, (decimal)avgPrice);
    }
}
