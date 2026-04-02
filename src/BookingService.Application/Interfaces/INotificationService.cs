namespace BookingService.Application.Services
{
    /// <summary>
    /// Defines the contract for sending notifications via different channels.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a notification message to a specified recipient (e.g., email address or Telegram username/chat ID).
        /// </summary>
        /// <param name="recipientIdentifier">The identifier of the recipient (e.g., email, Telegram chat ID).</param>
        /// <param name="subject">The subject of the notification (primarily for email).</param>
        /// <param name="message">The main content of the notification.</param>
        /// <param name="channel">Specifies the notification channel to use (e.g., Email, Telegram).</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if the channel is not supported or recipientIdentifier is invalid for the channel.</exception>
        Task SendNotificationAsync(string recipientIdentifier, string subject, string message, NotificationChannel channel);

        /// <summary>
        /// Sends an email notification.
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string body);

        /// <summary>
        /// Sends a Telegram notification.
        /// </summary>
        /// <param name="chatId">The Telegram chat ID of the recipient.</param>
        /// <param name="message">The message to send.</param>
        Task SendTelegramAsync(string chatId, string message);
    }

    /// <summary>
    /// Enum to specify the notification channel.
    /// </summary>
    public enum NotificationChannel
    {
        Email,
        Telegram,
        // Add other channels here like SMS, Push Notifications, etc.
    }
}