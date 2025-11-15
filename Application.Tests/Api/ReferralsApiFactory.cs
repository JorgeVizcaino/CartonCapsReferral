using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data;

namespace Application.Tests.Api;

public sealed class ReferralsApiFactory : WebApplicationFactory<api.Program>
{
    private SqliteConnection? sharedConnection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.RemoveAll(typeof(IAppDbContext));

            services.AddSingleton(provider =>
            {
                sharedConnection ??= new SqliteConnection("Filename=:memory:");
                if (sharedConnection.State != System.Data.ConnectionState.Open)
                {
                    sharedConnection.Open();
                }

                return sharedConnection;
            });

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var connection = sp.GetRequiredService<SqliteConnection>();
                options.UseSqlite(connection);
            });

            services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (Services is null)
        {
            throw new InvalidOperationException("Factory is not initialized.");
        }

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureDeletedAsync();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            sharedConnection?.Dispose();
            sharedConnection = null;
        }
    }
}
