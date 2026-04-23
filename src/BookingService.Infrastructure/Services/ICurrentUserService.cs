// Core/Application/Common/Interfaces/ICurrentUserService.cs

using BookingService.Application.Models.Kratos;

namespace BookingService.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    void SetCurrentUser(KratosSession session);
}
