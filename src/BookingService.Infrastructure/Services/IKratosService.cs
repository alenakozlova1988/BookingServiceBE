using BookingService.Application.Models;

namespace BookingService.Application.Interfaces;

public interface IKratosService
{
    Task<KratosSession?> GetSessionAsync(string? sessionToken = null);
    Task<string> GetLoginFlowAsync();
    Task<string> GetRegistrationFlowAsync();
    Task LogoutAsync(string sessionToken);
}
