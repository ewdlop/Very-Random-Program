using Microsoft.JSInterop;

namespace 亂七八糟.RazorClassLibrary;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.


public class ExampleJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    /// <summary>
    /// https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.javascript?view=net-9.0
    /// </summary>
    /// <param name="jsRuntime"></param>
    public ExampleJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new (() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/亂七八糟.RazorClassLibrary/exampleJsInterop.js").AsTask());
    }

    public async ValueTask<string> Prompt(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
