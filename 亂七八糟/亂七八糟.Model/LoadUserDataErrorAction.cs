namespace 亂七八糟.Model;

public record LoadUserDataErrorAction(string ErrorMessage, Exception Exception, Action<Exception>? OnError);
