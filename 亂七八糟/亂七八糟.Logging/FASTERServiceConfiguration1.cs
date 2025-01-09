namespace 亂七八糟.Logging;


using Microsoft.Extensions.Logging;

public class FASTERServiceConfiguration
{
    public int EventId { get; set; }

    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
    {
        [LogLevel.Information] = ConsoleColor.Green
    };
}
