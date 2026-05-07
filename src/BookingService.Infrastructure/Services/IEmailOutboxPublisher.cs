namespace BookingService.Application.Interfaces;

public interface IEmailOutboxPublisher
{
    Task PublishPendingAsync();
}