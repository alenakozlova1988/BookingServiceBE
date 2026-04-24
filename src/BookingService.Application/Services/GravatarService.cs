using System.Security.Cryptography;
using System.Text;

namespace BookingService.Application.Services;

public class GravatarService : IGravatarService
{
    public string GenerateGravatarUrl(string email)
    {
        var hash = string.Join("", MD5.Create()
            .ComputeHash(Encoding.UTF8.GetBytes(email.Trim().ToLower()))
            .Select(b => b.ToString("x2")));

        return $"https://www.gravatar.com/avatar/{hash}?d=identicon";
    } 
}