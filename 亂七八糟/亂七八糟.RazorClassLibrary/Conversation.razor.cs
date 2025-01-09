using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace 亂七八糟.RazorClassLibrary;

public partial class Conversation : Logging<Conversation>
{
    protected string UserInput { get; set; } = string.Empty;
    protected bool IsProcessing { get; set; }
    protected List<Message?> ConversationHistory { get; set; } = [];

    public const string API_AI = "/api/ai";
    public const string MARKDOWN_TO_HTML = "markdownToHtml";
    public const string SCROLL_TO_BOTTOM = "scrollToBottom";
    public const string USER = "user";
    public const string ASSISTANT = "assistant";

    protected ElementReference _conversationRef;
    protected CancellationTokenSource _cts = new();

    [Inject]
    public required HttpClient Http { get; init; }

    [Inject]
    public required IJSRuntime JSRuntime { get; init; }

    [Parameter]
    public string ApiEndpoint { get; set; } = API_AI;

    [Parameter]
    public EventCallback<string?> OnResponse { get; set; }

    [Parameter]
    public int MaxConversationLength { get; set; } = 100;

    [Parameter]
    public bool EnableMarkdown { get; set; } = true;

    protected record Message(string Content, string FormattedContent, bool IsUser, DateTime Timestamp)
    {
        protected Message(string content, string formattedContent, bool isUser) : this(content, formattedContent, isUser, DateTime.UtcNow)
        {
            Content = content;
            FormattedContent = formattedContent;
            IsUser = isUser;
        }
        public static Message Create(string content, string formattedContent, bool isUser) => new Message(content, formattedContent, isUser);
        
        public static Message Empty => new Message(string.Empty, string.Empty, false, DateTime.UnixEpoch);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (ConversationHistory.Count != 0)
        {
            await JSRuntime.InvokeVoidAsync(SCROLL_TO_BOTTOM, _conversationRef);
        }
    }

    protected virtual ValueTask HandleKeyPressValueAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && e.CtrlKey)
        {
            return ValueProcessInputAsync();
        }
        return ValueTask.CompletedTask;
    }

    protected virtual Task HandleKeyPressAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && e.CtrlKey)
        {
            return ProcessInputAsync();
        }
        return Task.CompletedTask;
    }

    protected virtual async Task ProcessInputAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput) || IsProcessing)
            return;

        try
        {
            IsProcessing = true;
            Message? userMessage = Message.Create(UserInput, UserInput, true);
            ConversationHistory.Add(userMessage);

            (string prompt, List<(string role, string content)> conversationHistory) request = 
            (
                prompt : UserInput,
                conversationHistory : ConversationHistory
                    .Select(m => (role : (m?.IsUser ?? false) ? USER : ASSISTANT, content : m.Content ))
                    .ToList()
            );

            _cts = new CancellationTokenSource();
            HttpResponseMessage? response = await Http.PostAsJsonAsync(ApiEndpoint, request, _cts.Token);
            response.EnsureSuccessStatusCode();

            AIResponse? result = await response.Content.ReadFromJsonAsync<AIResponse>() ?? default;
            string? formattedContent = EnableMarkdown ?
                await JSRuntime.InvokeAsync<string>(MARKDOWN_TO_HTML, result?.Response ?? default) :
                result?.Response;

            Message? aiMessage = Message.Create(result?.Response ?? string.Empty, formattedContent ?? string.Empty, false);

            await InvokeAsync(() =>
            {
                ConversationHistory.Add(aiMessage);
                if (ConversationHistory.Count > MaxConversationLength)
                {
                    ConversationHistory = ConversationHistory
                        .Skip(ConversationHistory.Count - MaxConversationLength)
                        .ToList();
                }
                StateHasChanged();
            });


            await OnResponse.InvokeAsync(result?.Response);

            _ = InvokeAsync(() =>
            {
                UserInput = string.Empty;
                StateHasChanged();
            });
        }
        catch (Exception ex)
        {
            Message? errorMessage = Message.Create($"Error: {ex.Message}", $"Error: {ex.Message}", false);
            _ = InvokeAsync(() =>
            {
                ConversationHistory.Add(errorMessage);
                StateHasChanged();
            });
        }
        finally
        {
            _ = InvokeAsync(() =>
            {
                IsProcessing = false;
                StateHasChanged();
            });
        }
    }

    protected virtual async ValueTask ValueProcessInputAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput) || IsProcessing)
            return;

        try
        {
            IsProcessing = true;
            Message? userMessage = Message.Create(UserInput, UserInput, true);
            ConversationHistory.Add(userMessage);

            (string prompt, List<(string role, string content)> conversationHistory) request =
            (
                prompt: UserInput,
                conversationHistory: ConversationHistory
                    .Select(m => (role: (m?.IsUser?? false) ? USER : ASSISTANT, content: m.Content))
                    .ToList()
            );

            _cts = new CancellationTokenSource();
            HttpResponseMessage? response = await Http.PostAsJsonAsync(ApiEndpoint, request, _cts.Token);
            response.EnsureSuccessStatusCode();

            AIResponse? result = await response.Content.ReadFromJsonAsync<AIResponse>() ?? default;
            string? formattedContent = EnableMarkdown ?
                await JSRuntime.InvokeAsync<string>(MARKDOWN_TO_HTML, result?.Response ?? default) :
                result?.Response;

            Message? aiMessage = Message.Create(result?.Response ?? string.Empty, formattedContent ?? string.Empty, false);

            await InvokeAsync(()=> {
                ConversationHistory.Add(aiMessage);
            });

            if (ConversationHistory.Count > MaxConversationLength)
            {
                ConversationHistory = ConversationHistory
                    .Skip(ConversationHistory.Count - MaxConversationLength)
                    .ToList();
            }

            await OnResponse.InvokeAsync(result?.Response);
            _ = InvokeAsync(() =>
            {
                UserInput = string.Empty;
                StateHasChanged();
            });
        }
        catch (Exception ex)
        {
            Message? errorMessage = Message.Create($"Error: {ex.Message}", $"Error: {ex.Message}", false);
            ConversationHistory.Add(errorMessage);
        }
        finally
        {
            _ = InvokeAsync(() =>{
                IsProcessing = false;
                StateHasChanged();
            });
        }
    }

    protected void ClearConversation()
    {
        ConversationHistory.Clear();
        UserInput = "";
        IsProcessing = false;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    protected record AIResponse(string Response);
}