using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using BookingService.Application.Interfaces;
using BookingService.Infrastructure.Services;

namespace BookingService.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IKratosService _kratosService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IKratosService kratosService,
        ICurrentUserService currentUserService,
        ILogger<AuthController> logger)
    {
        _kratosService = kratosService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [HttpGet("session")]
    public async Task<IActionResult> GetSession()
    {
        try
        {
            // Получаем сессию из Kratos
            var session = await _kratosService.GetSessionAsync();

            if (session == null || !session.Active)
            {
                _logger.LogDebug("No active session found");
                return Ok(new { active = false });
            }

            _logger.LogDebug("Active session found for user {UserId}", 
                session.Identity?.Id);

            return Ok(new 
            {
                active = true,
                expires_at = session.ExpiresAt,
                user_id = session.Identity?.Id,
             //   authenticated_at = session.AuthenticatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking session");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        if (!_currentUserService.IsAuthenticated)
            return Unauthorized(new { message = "Not authenticated" });

        return Ok(new 
        {
            id = _currentUserService.UserId,
            email = _currentUserService.Email,
           // last_name = _currentUserService.,
            is_authenticated = _currentUserService.IsAuthenticated
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Получаем session cookie
            var sessionCookie = Request.Cookies["ory_kratos_session"];

            if (!string.IsNullOrEmpty(sessionCookie))
            {
                // Удаляем сессию в Kratos
                await _kratosService.LogoutAsync($"ory_kratos_session={sessionCookie}");
            }

            // Удаляем cookies на клиенте
            Response.Cookies.Delete("ory_kratos_session", new CookieOptions
            {
                Domain = "localhost",
                Path = "/",
                SameSite = SameSiteMode.Lax,
                HttpOnly = true
            });

            Response.Cookies.Delete("ory_kratos_remember_me", new CookieOptions
            {
                Domain = "localhost",
                Path = "/",
                SameSite = SameSiteMode.Lax,
                HttpOnly = true
            });

            return Ok(new { success = true, message = "Logged out" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return StatusCode(500, new { error = "Logout failed" });
        }
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateSession([FromBody] ValidateSessionRequest request)
    {
        if (string.IsNullOrEmpty(request?.SessionToken))
            return BadRequest(new { error = "Session token required" });

        try
        {
            var isValid = true;//await _kratosService.ValidateSessionAsync(request.SessionToken);
            return Ok(new { valid = isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session validation failed");
            return StatusCode(500, new { error = "Validation failed" });
        }
    }
}

public class ValidateSessionRequest
{
    public string? SessionToken { get; set; }
}
