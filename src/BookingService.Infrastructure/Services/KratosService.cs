// Services/KratosService.cs
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using BookingService.Application.Interfaces;
using Microsoft.AspNetCore.Http;

public class KratosService :  IKratosService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public KratosService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;

        // Настройка базового URL для Kratos Public API
        _httpClient.BaseAddress = new Uri("http://localhost:4433");
    }

    public async Task<KratosSession?> GetSessionAsync(string? sessionToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/sessions/whoami");

        // 1. Проверка куки из HTTP контекста
        if (_httpContextAccessor.HttpContext != null)
        {
            var cookie = _httpContextAccessor.HttpContext.Request.Cookies["ory_kratos_session"];
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", $"ory_kratos_session={cookie}");
            }
        }

        // 2. Проверка токена из заголовка
        if (!string.IsNullOrEmpty(sessionToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", sessionToken);
        }

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return null;

        response.EnsureSuccessStatusCode();

        var session = await response.Content.ReadFromJsonAsync<KratosSession>();
        return session;
    }

    public Task<string> GetLoginFlowAsync()
    {
        throw new NotImplementedException();
    }

    public Task<string> GetRegistrationFlowAsync()
    {
        throw new NotImplementedException();
    }

    public Task LogoutAsync(string sessionToken)
    {
        throw new NotImplementedException();
    }

    // Другие методы...
}
