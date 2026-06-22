using AutoConfig.Core.Enums;
using AutoConfig.Core.Exceptions;
using AutoConfig.Infrastructure.Repositories;
using AutoConfig.Infrastructure.Services;
using AutoConfig.Tests.Helpers;
using FluentAssertions;

namespace AutoConfig.Tests.Services;

public class QuoteServiceTests
{
    private static QuoteService CreateSut(out AutoConfig.Infrastructure.Data.AppDbContext db)
    {
        db = TestDbContextFactory.Create();
        return new QuoteService(new QuoteRepository(db), new ConfigurationRepository(db));
    }

    [Fact]
    public async Task Create_ValidConfig_ReturnsQuote()
    {
        var sut = CreateSut(out var db);
        var (user, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(db);

        var quote = await sut.CreateAsync(user.Id, config.Id, "Please deliver ASAP");

        quote.ConfigurationId.Should().Be(config.Id);
        quote.Status.Should().Be(QuoteStatus.Pending);
        quote.TotalPrice.Should().Be(config.TotalPrice);
        quote.FinalPrice.Should().Be(config.TotalPrice);
        quote.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(89));
    }

    [Fact]
    public async Task Create_ConfigNotOwned_ThrowsForbiddenException()
    {
        var sut = CreateSut(out var db);
        var (_, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(db);

        await sut.Invoking(s => s.CreateAsync(Guid.NewGuid(), config.Id, "notes"))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Create_NonExistentConfig_ThrowsNotFoundException()
    {
        var sut = CreateSut(out _);

        await sut.Invoking(s => s.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), "notes"))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAdmin_AppliesDiscountCorrectly()
    {
        var sut = CreateSut(out var db);
        var (user, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(db);
        var quote = await sut.CreateAsync(user.Id, config.Id, "");

        var updated = await sut.UpdateAdminAsync(quote.Id, QuoteStatus.Approved, 10m, "Sconto fedeltà");

        updated.Status.Should().Be(QuoteStatus.Approved);
        updated.Discount.Should().Be(10m);
        updated.FinalPrice.Should().BeApproximately(config.TotalPrice * 0.9m, 0.01m);
        updated.AdminNotes.Should().Be("Sconto fedeltà");
    }

    [Fact]
    public async Task Get_WrongUser_ThrowsForbiddenException()
    {
        var sut = CreateSut(out var db);
        var (user, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(db);
        var quote = await sut.CreateAsync(user.Id, config.Id, "");

        await sut.Invoking(s => s.GetAsync(quote.Id, Guid.NewGuid(), false))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Get_Admin_CanAccessAnyQuote()
    {
        var sut = CreateSut(out var db);
        var (user, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(db);
        var quote = await sut.CreateAsync(user.Id, config.Id, "");

        var result = await sut.GetAsync(quote.Id, Guid.NewGuid(), isAdmin: true);

        result.Id.Should().Be(quote.Id);
    }

    [Fact]
    public async Task Delete_NotOwner_ThrowsForbiddenException()
    {
        var sut = CreateSut(out var db);
        var (user, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(db);
        var quote = await sut.CreateAsync(user.Id, config.Id, "");

        await sut.Invoking(s => s.DeleteAsync(quote.Id, Guid.NewGuid(), false))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetUserQuotes_ReturnsOnlyUserQuotes()
    {
        var sut = CreateSut(out var db);
        var (user, model, mot, config) = await TestDataBuilder.SeedBasicConfigAsync(db);
        var otherUser = TestDataBuilder.RegularUser("other@test.it");
        var otherConfig = TestDataBuilder.Configuration(otherUser.Id, model.Id, mot.Id);
        db.Users.Add(otherUser);
        db.Configurations.Add(otherConfig);
        await db.SaveChangesAsync();

        await sut.CreateAsync(user.Id, config.Id, "");
        await sut.CreateAsync(otherUser.Id, otherConfig.Id, "");

        var quotes = await sut.GetUserQuotesAsync(user.Id);
        quotes.Should().HaveCount(1);
        quotes[0].UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Get_Owner_ReturnsQuote()
    {
        var sut = CreateSut(out var db);
        var (user, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(db);
        var quote = await sut.CreateAsync(user.Id, config.Id, "nota test");

        var result = await sut.GetAsync(quote.Id, user.Id, isAdmin: false);

        result.Id.Should().Be(quote.Id);
    }

    [Fact]
    public async Task UpdateAdmin_NonExistentQuote_ThrowsNotFoundException()
    {
        var sut = CreateSut(out _);

        await sut.Invoking(s => s.UpdateAdminAsync(Guid.NewGuid(), QuoteStatus.Approved, 0m, ""))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAdmin_ZeroDiscount_FinalPriceEqualsTotal()
    {
        var sut = CreateSut(out var db);
        var (user, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(db);
        var quote = await sut.CreateAsync(user.Id, config.Id, "");

        var updated = await sut.UpdateAdminAsync(quote.Id, QuoteStatus.Approved, 0m, "");

        updated.FinalPrice.Should().Be(updated.TotalPrice);
    }

    [Fact]
    public async Task Delete_Owner_Succeeds()
    {
        var sut = CreateSut(out var db);
        var (user, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(db);
        var quote = await sut.CreateAsync(user.Id, config.Id, "");

        await sut.DeleteAsync(quote.Id, user.Id, isAdmin: false);

        await sut.Invoking(s => s.GetAsync(quote.Id, user.Id, isAdmin: false))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_Admin_CanDeleteAnyQuote()
    {
        var sut = CreateSut(out var db);
        var (user, _, _, config) = await TestDataBuilder.SeedBasicConfigAsync(db);
        var quote = await sut.CreateAsync(user.Id, config.Id, "");

        await sut.DeleteAsync(quote.Id, Guid.NewGuid(), isAdmin: true);

        await sut.Invoking(s => s.GetAsync(quote.Id, user.Id, isAdmin: false))
            .Should().ThrowAsync<NotFoundException>();
    }
}
