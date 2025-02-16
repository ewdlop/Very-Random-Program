using 亂七八糟.Shared.Interfaces;

namespace 亂七八糟.Model;

public record Callbacks<T1, T2, T3>
(
    Action<T1> ? OnSuccessAction = null,
    Action<Exception> ? OnErrorAction = null,
    Func<T1, Task<T2>> ? OnSuccessTask = null,
    Func<T1, Task<Exception>> ? OnErrorTask = null,
    Func<T1, ValueTask<T3>> ? OnSuccessValueTask = null,
    Func<T1, ValueTask<Exception>> ? OnErrorValueTask = null
) : ICallbacks<T1, T2, T3>;
