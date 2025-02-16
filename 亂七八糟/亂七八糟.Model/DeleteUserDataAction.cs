namespace 亂七八糟.Model;

public record DeleteUserDataAction(Action? OnSuccess = null, Action<Exception>? OnError = null);
