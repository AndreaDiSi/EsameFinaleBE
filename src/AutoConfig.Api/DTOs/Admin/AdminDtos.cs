namespace AutoConfig.Api.DTOs.Admin;

public record DashboardStatsDto(
    int TotalUsers, int TotalConfigurations, int TotalQuotes,
    int PendingQuotes, int ApprovedQuotes,
    decimal TotalRevenue, decimal AverageConfigurationPrice);
