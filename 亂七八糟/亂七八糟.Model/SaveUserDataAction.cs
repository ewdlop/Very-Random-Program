namespace 亂七八糟.Model;

public record SaveUserDataAction<T1,T2,T3>(
    string Username,
    Dictionary<string, string> Settings,
    Action<T1>? OnSuccess = null,
    Action<Exception>? OnError = null
) : Callbacks<T1,T2,T3>(OnSuccess, OnError);