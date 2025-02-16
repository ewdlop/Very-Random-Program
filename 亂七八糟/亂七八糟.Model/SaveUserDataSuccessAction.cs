namespace 亂七八糟.Model;

public record SaveUserDataSuccessAction<TSender>(UserState SavedState, Action<UserState>? OnSuccess, TSender? Sender) : Sender<TSender?>(Sender);
