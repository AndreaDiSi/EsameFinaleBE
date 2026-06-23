using AutoConfig.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutoConfig.Tests.Integration;

public class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    public Task InitializeAsync() => Task.CompletedTask;

    public new Task DisposeAsync() => base.DisposeAsync().AsTask();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
<<<<<<< HEAD
        builder.UseContentRoot(AppContext.BaseDirectory);
=======
        var envPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "src", "AutoConfig.Api", ".env");
        DotNetEnv.Env.Load(Path.GetFullPath(envPath));

>>>>>>> 83139a211c7e1e4b4cf7931d99d8ba26ade67cb0
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase(_dbName));
        });
    }
}
