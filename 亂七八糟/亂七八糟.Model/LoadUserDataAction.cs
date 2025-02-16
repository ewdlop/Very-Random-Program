namespace 亂七八糟.Model;

// Actions with Callbacks
public record LoadUserDataAction(Action<UserState>? OnSuccess = null, Action<Exception>? OnError = null);