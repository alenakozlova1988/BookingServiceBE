// Infrastructure/Extensions/ServiceCollectionExtensions.cs

using BookingService.Application.Interfaces;
using BookingService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Регистрация HttpClient для Kratos
        services.AddHttpClient<IKratosService, KratosService>(client =>
        {
            var kratosUrl = configuration["Kratos:BaseUrl"] 
                            ?? "http://localhost:4433";
            client.BaseAddress = new Uri(kratosUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Регистрация CurrentUserService как Scoped
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}