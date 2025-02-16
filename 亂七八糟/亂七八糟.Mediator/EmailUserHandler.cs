using MediatR;
using Microsoft.Extensions.Logging;
using NETCore.MailKit.Core;
using 亂七八糟.Model;

namespace 亂七八糟.Mediator;

public class EmailUserHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailUserHandler> _logger;

    public EmailUserHandler(IEmailService emailService, ILogger<EmailUserHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Avoid multiple responsibilities in a single method.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            string mailTo = notification.Email;
            string subject = "Welcome to our platform!";
            string message = "Welcome to our platform!";
            await _emailService.SendAsync(mailTo, subject, message);

            var result = new UserCreatedResult(
                EmailSent: true,
                UserId: notification.UserId
            );

            notification.OnSuccess?.Invoke(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", notification.Email);
            notification.OnError?.Invoke(ex);
        }
    }
}