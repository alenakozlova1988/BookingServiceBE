// Infrastructure/Middleware/KratosSessionMiddleware.cs
using BookingService.Application.Common.Interfaces;
using BookingService.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
namespace BookingService.Infrastructure.Middlewares;

public class KratosSessionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Разрешаем сервисы через Service Locator pattern
        var serviceProvider = context.RequestServices;
        var kratosService = serviceProvider.GetRequiredService<IKratosService>();
        var currentUserService = serviceProvider.GetRequiredService<ICurrentUserService>();
        
        try
        {
            var session = await kratosService.GetSessionAsync();

            if (session != null && session.Active)
            {
                currentUserService.SetCurrentUser(session);
                context.Items["KratosSession"] = session;
                context.Items["UserId"] = session.Identity.Id;
            }
        }
        catch (Exception ex)
        {
            // Логирование
        }

        await next(context);
    }
}