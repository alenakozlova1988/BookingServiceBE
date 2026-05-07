// Infrastructure/Middleware/KratosSessionMiddleware.cs
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Domain.Interfaces;
using BookingService.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookingService.Infrastructure.Middlewares;

public class KratosSessionMiddleware(
    RequestDelegate next,
    ILogger<KratosSessionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<KratosSessionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        var method = context.Request.Method;

        _logger.LogInformation(
            "Начало обработки запроса: {Method} {Path}",
            method, path);

        // Разрешаем сервисы через Service Locator pattern
        var serviceProvider = context.RequestServices;
        var kratosService = serviceProvider.GetRequiredService<IKratosService>();
        var currentUserService = serviceProvider.GetRequiredService<ICurrentUserService>();

        try
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogDebug("Попытка получения сессии Kratos для запроса {Method} {Path}", method, path);

            var session = await kratosService.GetSessionAsync();

            stopwatch.Stop();

            _logger.LogDebug("Запрос к Kratos занял {ElapsedMs}мс", stopwatch.ElapsedMilliseconds);

            if (session != null && session.Active)
            {
                var userId = Guid.Parse(session.Identity.Id);
                using (var scope = context.RequestServices.CreateScope())
                {
                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                    var existingUser = await userRepository.GetByIdAsync(userId);
                   
                   
                    // Если нет такого пользователя, то сохраняем его в базу
                    if (existingUser == null)
                    {
                        var newUser = GetUserFromSession(session);
                        await userRepository.AddAsync(newUser);
                    }
                }

                _logger.LogInformation(
                    "Сессия найдена. ID сессии: {SessionId}, Активна: {IsActive}, UserId: {UserId}",
                    session.Id,
                    session.Active,
                    session.Identity?.Id);

                currentUserService.SetCurrentUser(session);
                context.Items["KratosSession"] = session;
                context.Items["UserId"] = session.Identity.Id;

                _logger.LogInformation(
                    "Пользователь {UserId} успешно авторизован через Kratos",
                    session.Identity.Id);
            }
            else
            {
                _logger.LogWarning(
                    "Сессия {SessionId} не найдена или  не активна (истекла/завершена)",
                    session.Id);
            }

            // 2. СОЗДАЕМ CLAIMS (Магия .NET)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, session.Identity.Id),
                //new Claim(ClaimTypes.Email, session.Identity.Traits? ?? ""),
                // Если в Kratos есть метаданные с ролями:
                // new Claim(ClaimTypes.Role, session.Identity.MetadataPublic?["role"]?.ToString() ?? "user")
            };

            // "KratosAuth" — это имя схемы аутентификации
            var identity = new ClaimsIdentity(claims, "KratosAuth");
            context.User = new ClaimsPrincipal(identity);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Ошибка HTTP при обращении к Kratos. Метод: {Method}, Path: {Path}",
                method, path);

            // Можно добавить повторную попытку или fallback
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(
                ex,
                "Таймаут при обращении к Kratos. Метод: {Method}, Path: {Path}",
                method, path);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Неожиданная ошибка в middleware Kratos. Метод: {Method}, Path: {Path}",
                method, path);

            // Для продакшена
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Детали ошибки для отладки: {StackTrace}",
                    ex.StackTrace);
            }
        }

        _logger.LogInformation(
            "Завершение обработки запроса: {Method} {Path}",
            method, path);
        
        await _next(context);
    }

    private static User GetUserFromSession(KratosSession session)
    {
        var traitsJson = session.Identity.Traits;
        var traitsElement = traitsJson.RootElement;
        
        var email = traitsElement.TryGetProperty("email", out var emailProp) 
            ? emailProp.GetString() ?? "" 
            : "";
                        
        var firstName = traitsElement.TryGetProperty("first_name", out var firstNameProps) 
            ? firstNameProps.GetString() ?? "" 
            : "";
                        
        var lastName = traitsElement.TryGetProperty("first_name", out var lastNameProps) 
            ? lastNameProps.GetString() ?? "" 
            : "";
        
        // Создаем нового пользователя с данными из Kratos
        return  new User
        {
            Id = Guid.Parse(session.Identity.Id),
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = String.Empty,
            UserStatus = UserStatus.Active
        };
    }
}