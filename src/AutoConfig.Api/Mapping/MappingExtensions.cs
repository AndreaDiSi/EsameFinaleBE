using AutoConfig.Api.DTOs.Auth;
using AutoConfig.Api.DTOs.Catalog;
using AutoConfig.Api.DTOs.Configurations;
using AutoConfig.Api.DTOs.Quotes;
using AutoConfig.Api.DTOs.Users;
using AutoConfig.Core.Entities;

namespace AutoConfig.Api.Mapping;

public static class MappingExtensions
{
    public static UserPayload ToPayload(this User u) =>
        new(u.Id, u.Email, u.Name, u.Role.ToString(), u.CreatedAt);

    public static UserDto ToDto(this User u) =>
        new(u.Id, u.Email, u.Name, u.Role.ToString(), u.CreatedAt);

    public static CarModelDto ToDto(this CarModel m, bool includeMots = false) =>
        new(m.Id, m.Name, m.Brand, m.Category.ToString(), m.BasePrice, m.Description, m.ImageColor,
            includeMots ? m.Motorizations.Select(x => x.ToDto()).ToList() : null);

    public static MotorizationDto ToDto(this Motorization m) =>
        new(m.Id, m.ModelId, m.Name, m.FuelType.ToString(), m.Power, m.Torque, m.Acceleration, m.Consumption, m.Price);

    public static CarOptionDto ToDto(this CarOption o) =>
        new(o.Id, o.Name, o.Description, o.Category.ToString(), o.Price, o.Color,
            o.IncompatibleWith.Select(x => x.Id).ToList(),
            o.RequiredMotorizations.Select(m => m.Id).ToList());

    public static ConfigurationDto ToDto(this VehicleConfiguration c) =>
        new(c.Id, c.UserId, c.Name,
            c.ModelId, c.Model?.Name ?? "", c.Model?.Brand ?? "",
            c.MotorizationId, c.Motorization?.Name ?? "",
            c.Options.Select(o => o.Id).ToList(),
            c.TotalPrice, c.CreatedAt, c.UpdatedAt);

    public static QuoteDto ToDto(this Quote q) =>
        new(q.Id, q.ConfigurationId, q.UserId,
            q.TotalPrice, q.Discount, q.FinalPrice,
            q.Status.ToString(), q.Notes, q.AdminNotes,
            q.CreatedAt, q.UpdatedAt, q.ExpiresAt,
            q.Configuration?.Name);
}
