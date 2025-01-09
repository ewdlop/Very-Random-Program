using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace 亂七八糟.RazorClassLibrary;

public abstract class Logging<T>: ComponentBase
{
    [Inject]
    public required ILogger<T> Logger { get; init; }
}
