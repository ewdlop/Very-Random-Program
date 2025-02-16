namespace 亂七八糟.Model;

public record LoadUserDataSuccessAction(UserState LoadedState, Action<UserState>? OnSuccess);
