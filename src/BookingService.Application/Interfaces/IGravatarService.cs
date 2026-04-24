namespace BookingService.Application.Services;

public interface IGravatarService
{
    string GenerateGravatarUrl(string email);
}