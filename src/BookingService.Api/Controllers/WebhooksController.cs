using System.Text;
using System.Text.Json;
using BookingService.Application.Dto;
using BookingService.Application.Interfaces;
using BookingService.Application.Services;

namespace BookingService.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly IGravatarService _gravatarService;
    private readonly HttpClient _httpClient;
    private readonly IKratosService _kratosService;

    public WebhooksController(IKratosService kratosService, IGravatarService gravatarService,
        IHttpClientFactory httpClientFactory)
    {
        _gravatarService = gravatarService;
        _httpClient = httpClientFactory.CreateClient();
        _kratosService = kratosService;
    }

    [HttpPost("generate-avatar")]
    public async Task<IActionResult> GenerateAvatar([FromBody] KratosRegistrationWebhookModel model)
    {
        var secret = "Bearer dGhpcy1pcy1hLXNlY3JldC10b2tlbi0xMjM0NTY=";
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader != secret)
            return Unauthorized();

        // 1. Получаем URL
        var imageUrl = _gravatarService.GenerateGravatarUrl(model.Email);

        // Убедитесь, что URL Admin API прописан в appsettings.json
        var adminApiUrl = "http://localhost:4434/identities/" + model.UserId;

        var patchData = new[]
        {
            new
            {
                op = "add", // операция добавления
                path = "/traits/avatar_url", // путь к полю в JSON пользователя
                value = imageUrl
            }
        };

        // 4. Отправляем в Kratos Admin API
        var response = await _httpClient.PatchAsync(
            adminApiUrl,
            new StringContent(JsonSerializer.Serialize(patchData), Encoding.UTF8, "application/json-patch+json")
        );

        if (response.IsSuccessStatusCode)
        {
            return Ok();
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, error);
        }
    }
}