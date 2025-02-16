namespace 亂七八糟.Shared.Interfaces;

public interface ICallbacks<T1, T2, T3>
{
    Action<T1>? OnSuccessAction { get; }
    Action<Exception>? OnErrorAction { get; }
    Func<T1, Task<T2>>? OnSuccessTask { get; }
    Func<T1, Task<Exception>>? OnErrorTask { get; }
    Func<T1, ValueTask<T3>>? OnSuccessValueTask { get; }
    Func<T1, ValueTask<Exception>>? OnErrorValueTask { get; }
}
