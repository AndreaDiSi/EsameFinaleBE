using AutoConfig.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutoConfig.Tests.Integration;

public class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    public Task InitializeAsync()
    {
        // Load .env before the test host starts so env vars are available
        // when WebApplication.CreateBuilder picks them up via AddEnvironmentVariables()
        var envPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "src", "AutoConfig.Api", ".env"));

        if (File.Exists(envPath))
        {
            foreach (var line in File.ReadAllLines(envPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
                var idx = line.IndexOf('=');
                if (idx > 0)
                    Environment.SetEnvironmentVariable(line[..idx].Trim(), line[(idx + 1)..].Trim());
            }
        }

        return Task.CompletedTask;
    }

    public new Task DisposeAsync() => base.DisposeAsync().AsTask();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(AppContext.BaseDirectory);
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase(_dbName));
        });
    }
}
