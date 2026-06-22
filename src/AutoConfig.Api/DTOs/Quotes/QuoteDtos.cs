using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AutoConfig.Api.DTOs.Quotes;

public record QuoteDto(
    Guid Id, Guid ConfigurationId, Guid UserId,
    decimal TotalPrice, decimal Discount, decimal FinalPrice,
    string Status, string Notes, string AdminNotes,
    DateTime CreatedAt, DateTime UpdatedAt, DateTime ExpiresAt,
    string? ConfigurationName = null);

public record CreateQuoteRequest(
    [property: JsonRequired][Required] Guid ConfigurationId,
    [MaxLength(500)] string Notes = "");

public record UpdateQuoteAdminRequest(
    [Required] string Status,
    [property: JsonRequired][Required, Range(0, 50)] decimal Discount,
    [MaxLength(500)] string AdminNotes = "");
