namespace BookingService.Application.Interfaces;

public interface IEmailTemplateService
{
    /// <summary>
    /// Заполнить шаблон данными
    /// </summary>
    Task<string> RenderTemplateAsync<T>(string templateName, T model, CancellationToken cancellationToken = default);
}