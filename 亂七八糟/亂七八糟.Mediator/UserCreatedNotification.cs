using 亂七八糟.Model;

namespace 亂七八糟.Mediator
{
    // Example domain event
    public class UserCreatedNotification : NotificationWithCallback<UserCreatedResult>
    {
        public string UserId { get; }
        public string Email { get; }

        public UserCreatedNotification(
            string userId,
            string email,
            Action<UserCreatedResult>? onSuccess = null,
            Action<Exception>? onError = null)
            : base(onSuccess, onError)
        {
            UserId = userId;
            Email = email;
        }
    }
}
