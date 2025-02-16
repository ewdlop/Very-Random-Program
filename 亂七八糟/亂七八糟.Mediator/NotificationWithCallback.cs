using MediatR;

namespace 亂七八糟.Mediator
{
    public abstract class NotificationWithCallback<TResult> : INotification
    {
        public Action<TResult>? OnSuccess { get; }
        public Action<Exception>? OnError { get; }

        protected NotificationWithCallback(Action<TResult>? onSuccess = null, Action<Exception>? onError = null)
        {
            OnSuccess = onSuccess;
            OnError = onError;
        }
    }
}
