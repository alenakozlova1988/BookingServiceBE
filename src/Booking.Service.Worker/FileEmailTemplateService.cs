using BookingService.Application.Interfaces;
using RazorLight;

namespace Booking.Service.Worker;

public class FileEmailTemplateService : IEmailTemplateService
{
    // Путь будет вычисляться относительно места запуска приложения
    private readonly RazorLightEngine _engine = new RazorLightEngineBuilder()
        .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates")) 
        .UseMemoryCachingProvider()
        .Build();
    
    public async Task<string> RenderTemplateAsync<T>(string templateName, T model, CancellationToken cancellationToken = default)
    {
            templateName = "BookingConfirmation";
            return await _engine.CompileRenderAsync(templateName, model);
    }
}