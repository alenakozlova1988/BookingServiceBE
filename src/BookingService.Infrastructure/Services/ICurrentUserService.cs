namespace BookingService.Infrastructure.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    void SetCurrentUser(KratosSession session);
}