using Microsoft.AspNetCore.Http;

namespace BookingService.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private KratosSession? _currentSession;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _currentSession?.Identity.Id;
    public string? Email { get; }

    //public string? Email => _currentSession?.Identity.Traits.GetProperty("email").GetString();

    public bool IsAuthenticated => _currentSession?.Active == true;

    public void SetCurrentUser(KratosSession session)
    {
        _currentSession = session;
    }
}
