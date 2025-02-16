namespace 亂七八糟.Model;

public record UserState
{
    public string? Username { get; init; }
    public Dictionary<string, string> Settings { get; init; } = new();
    public bool IsLoading { get; init; }
    public string? Error { get; init; }
    public object? Cause { get; init; }
}
