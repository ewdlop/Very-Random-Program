namespace 亂七八糟.Model;

public record SaveUserDataErrorAction(string ErrorMessage, Exception Exception, Action<Exception>? OnError);