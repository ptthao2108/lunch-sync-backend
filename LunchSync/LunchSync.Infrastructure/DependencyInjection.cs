using LunchSync.Core.Common.Interfaces;
using LunchSync.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LunchSync.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);

                // Enable retry on failure
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            }));

        // Register IUnitOfWork yet done !!!
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<AppDbContext>());

        // ── Repositories ──
        //services.AddScoped<ISessionRepository, SessionRepository>();
        //services.AddScoped<IDishRepository, DishRepository>();
        //services.AddScoped<IRestaurantRepository, RestaurantRepository>();
        //services.AddScoped<ICollectionRepository, CollectionRepository>();

        // ── Caching ──
        //services.AddSingleton<IDishProfileCache, InMemoryDishProfileCache>();

        // ── Auth (Cognito) ──
        //services.AddScoped<ICognitoAuthProvider, CognitoAuthProvider>();


        return services;
    }
}

