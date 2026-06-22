using AutoConfig.Core.Enums;
using AutoConfig.Core.Exceptions;
using AutoConfig.Infrastructure.Repositories;
using AutoConfig.Infrastructure.Services;
using AutoConfig.Tests.Helpers;
using FluentAssertions;

namespace AutoConfig.Tests.Services;

public class UserServiceTests
{
    private static UserService CreateSut(out AutoConfig.Infrastructure.Data.AppDbContext db)
    {
        db = TestDbContextFactory.Create();
        return new UserService(new UserRepository(db));
    }

    [Fact]
    public async Task GetAll_ReturnsAllUsers()
    {
        var sut = CreateSut(out var db);
        db.Users.AddRange(TestDataBuilder.RegularUser("a@test.it"), TestDataBuilder.RegularUser("b@test.it"));
        await db.SaveChangesAsync();

        var users = await sut.GetAllAsync();

        users.Should().HaveCount(2);
    }

    [Fact]
    public async Task Get_NonExistent_ThrowsNotFoundException()
    {
        var sut = CreateSut(out _);

        await sut.Invoking(s => s.GetAsync(Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_ChangesRoleAndEmail()
    {
        var sut = CreateSut(out var db);
        var user = TestDataBuilder.RegularUser();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var updated = await sut.UpdateAsync(user.Id, "New Name", "new@test.it", Role.Admin);

        updated.Name.Should().Be("New Name");
        updated.Email.Should().Be("new@test.it");
        updated.Role.Should().Be(Role.Admin);
    }

    [Fact]
    public async Task Update_DuplicateEmail_ThrowsConflictException()
    {
        var sut = CreateSut(out var db);
        var user1 = TestDataBuilder.RegularUser("taken@test.it");
        var user2 = TestDataBuilder.RegularUser("mine@test.it");
        db.Users.AddRange(user1, user2);
        await db.SaveChangesAsync();

        await sut.Invoking(s => s.UpdateAsync(user2.Id, "Name", "taken@test.it", Role.User))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Delete_RemovesUser()
    {
        var sut = CreateSut(out var db);
        var user = TestDataBuilder.RegularUser();
        db.Users.Add(user);
        await db.SaveChangesAsync();

        await sut.DeleteAsync(user.Id);

        db.Users.Find(user.Id).Should().BeNull();
    }

    [Fact]
    public async Task Update_SameEmail_DoesNotThrow()
    {
        var sut = CreateSut(out var db);
        var user = TestDataBuilder.RegularUser("same@test.it");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var updated = await sut.UpdateAsync(user.Id, "Nuovo Nome", "same@test.it", Role.User);

        updated.Name.Should().Be("Nuovo Nome");
        updated.Email.Should().Be("same@test.it");
    }

    [Fact]
    public async Task Delete_NonExistent_ThrowsNotFoundException()
    {
        var sut = CreateSut(out _);

        await sut.Invoking(s => s.DeleteAsync(Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAll_NoUsers_ReturnsEmptyList()
    {
        var sut = CreateSut(out _);

        var users = await sut.GetAllAsync();

        users.Should().BeEmpty();
    }
}
