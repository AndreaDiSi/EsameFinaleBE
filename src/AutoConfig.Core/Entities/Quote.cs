using AutoConfig.Core.Enums;

namespace AutoConfig.Core.Entities;

public class Quote
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ConfigurationId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalPrice { get; set; }
    public QuoteStatus Status { get; set; } = QuoteStatus.Pending;
    public string Notes { get; set; } = string.Empty;
    public string AdminNotes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public VehicleConfiguration Configuration { get; set; } = null!;
    public User User { get; set; } = null!;
}
